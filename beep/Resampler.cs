﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//https://github.com/dnault/libresample4j

/******************************************************************************
 *
 * libresample4j
 * Copyright (c) 2009 Laszlo Systems, Inc. All Rights Reserved.
 *
 * libresample4j is a Java port of Dominic Mazzoni's libresample 0.1.3,
 * which is in turn based on Julius Smith's Resample 1.7 library.
 *      http://www-ccrma.stanford.edu/~jos/resample/
 *
 * License: LGPL -- see the file LICENSE.txt for more information
 *
 *****************************************************************************/
//package com.laszlosystems.libresample4j;

//import java.nio.FloatBuffer;

namespace beep
{

    public class Resampler
    {

        //public static class Result
        //{
        //    public readonly int inputSamplesConsumed;
        //    public readonly int outputSamplesGenerated;

        //    public Result(int inputSamplesConsumed, int outputSamplesGenerated)
        //    {
        //        this.inputSamplesConsumed = inputSamplesConsumed;
        //        this.outputSamplesGenerated = outputSamplesGenerated;
        //    }
        //}

        // number of values per 1/delta in impulse response
        internal static readonly int Npc = 4096;

        private readonly float[] Imp;
        private readonly float[] ImpD;
        private readonly float LpScl;
        private readonly int Nmult;
        private readonly int Nwing;
        private readonly double minFactor;
        private readonly double maxFactor;
        private readonly int XSize;
        private readonly float[] X;
        private int Xp; // Current "now"-sample pointer for input
        private int Xread; // Position to put new samples
        private readonly int Xoff;
        private readonly float[] Y;
        private int Yp;
        private double Time;

        /**
         * Clone an existing resampling session. Faster than creating one from scratch.
         *
         * @param other
         */
        public Resampler(Resampler other)
        {
            this.Imp = (float[])other.Imp.Clone();
            this.ImpD = (float[])other.ImpD.Clone();
            this.LpScl = other.LpScl;
            this.Nmult = other.Nmult;
            this.Nwing = other.Nwing;
            this.minFactor = other.minFactor;
            this.maxFactor = other.maxFactor;
            this.XSize = other.XSize;
            this.X = (float[])other.X.Clone();
            this.Xp = other.Xp;
            this.Xread = other.Xread;
            this.Xoff = other.Xoff;
            this.Y = (float[])other.Y.Clone();
            this.Yp = other.Yp;
            this.Time = other.Time;
        }

        /**
         * Create a new resampling session.
         *
         * @param highQuality true for better quality, slower processing time
         * @param minFactor   lower bound on resampling factor for this session
         * @param maxFactor   upper bound on resampling factor for this session
         * @throws ArgumentException if minFactor or maxFactor is not
         *                                  positive, or if maxFactor is less than minFactor
         */
        public Resampler(bool highQuality, double minFactor, double maxFactor)
        {
            if (minFactor <= 0.0 || maxFactor <= 0.0)
            {
                throw new ArgumentException("minFactor and maxFactor must be positive");
            }
            if (maxFactor < minFactor)
            {
                throw new ArgumentException("minFactor must be <= maxFactor");
            }

            this.minFactor = minFactor;
            this.maxFactor = maxFactor;
            this.Nmult = highQuality ? 35 : 11;
            this.LpScl = 1.0f;
            this.Nwing = Npc * (this.Nmult - 1) / 2; // # of filter coeffs in right wing

            double Rolloff = 0.90;
            double Beta = 6;

            double[] Imp64 = new double[this.Nwing];

            FilterKit.lrsLpFilter(Imp64, this.Nwing, 0.5 * Rolloff, Beta, Npc);
            this.Imp = new float[this.Nwing];
            this.ImpD = new float[this.Nwing];

            for (int i = 0; i < this.Nwing; i++)
            {
                this.Imp[i] = (float)Imp64[i];
            }

            // Storing deltas in ImpD makes linear interpolation
            // of the filter coefficients faster
            for (int i = 0; i < this.Nwing - 1; i++)
            {
                this.ImpD[i] = this.Imp[i + 1] - this.Imp[i];
            }

            // Last coeff. not interpolated
            this.ImpD[this.Nwing - 1] = -this.Imp[this.Nwing - 1];

            // Calc reach of LP filter wing (plus some creeping room)
            int Xoff_min = (int)(((this.Nmult + 1) / 2.0) * Math.Max(1.0, 1.0 / minFactor) + 10);
            int Xoff_max = (int)(((this.Nmult + 1) / 2.0) * Math.Max(1.0, 1.0 / maxFactor) + 10);
            this.Xoff = Math.Max(Xoff_min, Xoff_max);

            // Make the inBuffer size at least 4096, but larger if necessary
            // in order to store the minimum reach of the LP filter and then some.
            // Then allocate the buffer an extra Xoff larger so that
            // we can zero-pad up to Xoff zeros at the end when we reach the
            // end of the input samples.
            this.XSize = Math.Max(2 * this.Xoff + 10, 4096);
            this.X = new float[this.XSize + this.Xoff];
            this.Xp = this.Xoff;
            this.Xread = this.Xoff;

            // Make the outBuffer long enough to hold the entire processed
            // output of one inBuffer
            int YSize = (int)(((double)this.XSize) * maxFactor + 2.0);
            this.Y = new float[YSize];
            this.Yp = 0;

            this.Time = (double)this.Xoff; // Current-time pointer for converter
        }

        public int getFilterWidth()
        {
            return this.Xoff;
        }

        /**
         * Process a batch of samples. There is no guarantee that the input buffer will be drained.
         *
         * @param factor    factor at which to resample this batch
         * @param buffers   sample buffer for producing input and consuming output
         * @param lastBatch true if this is known to be the last batch of samples
         * @return true iff resampling is complete (ie. no input samples consumed and no output samples produced)
         */
        public bool process(double factor, SampleBuffers buffers, bool lastBatch)
        {
            if (factor < this.minFactor || factor > this.maxFactor)
            {
                throw new ArgumentException("factor " + factor + " is not between minFactor=" + minFactor
                        + " and maxFactor=" + maxFactor);
            }

            int outBufferLen = buffers.getOutputBufferLength();
            int inBufferLen = buffers.getInputBufferLength();

            float[] Imp = this.Imp;
            float[] ImpD = this.ImpD;
            float LpScl = this.LpScl;
            int Nwing = this.Nwing;
            bool interpFilt = false; // TRUE means interpolate filter coeffs

            int inBufferUsed = 0;
            int outSampleCount = 0;

            // Start by copying any samples still in the Y buffer to the output
            // buffer
            if ((this.Yp != 0) && (outBufferLen - outSampleCount) > 0)
            {
                int len = Math.Min(outBufferLen - outSampleCount, this.Yp);

                buffers.consumeOutput(this.Y, 0, len);
                //for (int i = 0; i < len; i++) {
                //    outBuffer[outBufferOffset + outSampleCount + i] = this.Y[i];
                //}

                outSampleCount += len;
                for (int i = 0; i < this.Yp - len; i++)
                {
                    this.Y[i] = this.Y[i + len];
                }
                this.Yp -= len;
            }

            // If there are still output samples left, return now - we need
            // the full output buffer available to us...
            if (this.Yp != 0)
            {
                return inBufferUsed == 0 && outSampleCount == 0;
            }

            // Account for increased filter gain when using factors less than 1
            if (factor < 1)
            {
                LpScl = (float)(LpScl * factor);
            }

            while (true)
            {

                // This is the maximum number of samples we can process
                // per loop iteration

                /*
                 * #ifdef DEBUG
                 * printf("XSize: %d Xoff: %d Xread: %d Xp: %d lastFlag: %d\n",
                 * this.XSize, this.Xoff, this.Xread, this.Xp, lastFlag); #endif
                 */

                // Copy as many samples as we can from the input buffer into X
                int len = this.XSize - this.Xread;

                if (len >= inBufferLen - inBufferUsed)
                {
                    len = inBufferLen - inBufferUsed;
                }

                buffers.produceInput(this.X, this.Xread, len);
                //for (int i = 0; i < len; i++) {
                //    this.X[this.Xread + i] = inBuffer[inBufferOffset + inBufferUsed + i];
                //}

                inBufferUsed += len;
                this.Xread += len;

                int Nx;
                if (lastBatch && (inBufferUsed == inBufferLen))
                {
                    // If these are the last samples, zero-pad the
                    // end of the input buffer and make sure we process
                    // all the way to the end
                    Nx = this.Xread - this.Xoff;
                    for (int i = 0; i < this.Xoff; i++)
                    {
                        this.X[this.Xread + i] = 0;
                    }
                }
                else
                {
                    Nx = this.Xread - 2 * this.Xoff;
                }

                /*
                 * #ifdef DEBUG fprintf(stderr, "new len=%d Nx=%d\n", len, Nx);
                 * #endif
                 */

                if (Nx <= 0)
                {
                    break;
                }

                // Resample stuff in input buffer
                int Nout;
                if (factor >= 1)
                { // SrcUp() is faster if we can use it */
                    Nout = lrsSrcUp(this.X, this.Y, factor, /* &this.Time, */Nx, Nwing, LpScl, Imp, ImpD, interpFilt);
                }
                else
                {
                    Nout = lrsSrcUD(this.X, this.Y, factor, /* &this.Time, */Nx, Nwing, LpScl, Imp, ImpD, interpFilt);
                }

                /*
                 * #ifdef DEBUG
                 * printf("Nout: %d\n", Nout);
                 * #endif
                 */

                this.Time -= Nx; // Move converter Nx samples back in time
                this.Xp += Nx; // Advance by number of samples processed

                // Calc time accumulation in Time
                int Ncreep = (int)(this.Time) - this.Xoff;
                if (Ncreep != 0)
                {
                    this.Time -= Ncreep; // Remove time accumulation
                    this.Xp += Ncreep; // and add it to read pointer
                }

                // Copy part of input signal that must be re-used
                int Nreuse = this.Xread - (this.Xp - this.Xoff);

                for (int i = 0; i < Nreuse; i++)
                {
                    this.X[i] = this.X[i + (this.Xp - this.Xoff)];
                }

                /*
                #ifdef DEBUG
                printf("New Xread=%d\n", Nreuse);
                #endif */

                this.Xread = Nreuse; // Pos in input buff to read new data into
                this.Xp = this.Xoff;

                this.Yp = Nout;

                // Copy as many samples as possible to the output buffer
                if (this.Yp != 0 && (outBufferLen - outSampleCount) > 0)
                {
                    len = Math.Min(outBufferLen - outSampleCount, this.Yp);

                    buffers.consumeOutput(this.Y, 0, len);
                    //for (int i = 0; i < len; i++) {
                    //    outBuffer[outBufferOffset + outSampleCount + i] = this.Y[i];
                    //}

                    outSampleCount += len;
                    for (int i = 0; i < this.Yp - len; i++)
                    {
                        this.Y[i] = this.Y[i + len];
                    }
                    this.Yp -= len;
                }

                // If there are still output samples left, return now,
                //   since we need the full output buffer available
                if (this.Yp != 0)
                {
                    break;
                }
            }

            return inBufferUsed == 0 && outSampleCount == 0;
        }

        /**
         * Process a batch of samples. Convenience method for when the input and output are both floats.
         *
         * @param factor       factor at which to resample this batch
         * @param inputBuffer  contains input samples in the range -1.0 to 1.0
         * @param outputBuffer output samples will be deposited here
         * @param lastBatch    true if this is known to be the last batch of samples
         * @return true iff resampling is complete (ie. no input samples consumed and no output samples produced)
         */
        public bool process(double factor, float[] inputBuffer, int inBufferOffset, int inBufferLen, bool lastBatch, float[] outputBuffer, int outBufferOffset, int outBufferLen)
        {
            SampleBuffers sampleBuffers = new SampleBuffers(inputBuffer, inBufferOffset, inBufferLen, outputBuffer, outBufferOffset, outBufferLen);
            return process(factor, sampleBuffers, lastBatch);
        }

        /**
         * Process a batch of samples. Alternative interface if you prefer to work with arrays.
         *
         * @param factor         resampling rate for this batch
         * @param inBuffer       array containing input samples in the range -1.0 to 1.0
         * @param inBufferOffset offset into inBuffer at which to start processing
         * @param inBufferLen    number of valid elements in the inputBuffer
         * @param lastBatch      pass true if this is the last batch of samples
         * @param outBuffer      array to hold the resampled data
         * @return the number of samples consumed and generated
         */
        //public Result processResult(double factor, float[] inBuffer, int inBufferOffset, int inBufferLen, bool lastBatch, float[] outBuffer, int outBufferOffset, int outBufferLen) {
        //    process(factor, inBuffer, inBufferOffset, inBufferLen, lastBatch, outBuffer, outBufferOffset, outBufferLen);

        //    return new Result(inputBuffer.position() - inBufferOffset, outputBuffer.position() - outBufferOffset);
        //}



        /*
         * Sampling rate up-conversion only subroutine; Slightly faster than
         * down-conversion;
         */
        private int lrsSrcUp(float[] X, float[] Y, double factor, int Nx, int Nwing, float LpScl, float[] Imp,
                             float[] ImpD, bool Interp)
        {

            float[] Xp_array = X;
            int Xp_index;

            float[] Yp_array = Y;
            int Yp_index = 0;

            float v;

            double CurrentTime = this.Time;
            double dt; // Step through input signal
            double endTime; // When Time reaches EndTime, return to user

            dt = 1.0 / factor; // Output sampling period

            endTime = CurrentTime + Nx;
            while (CurrentTime < endTime)
            {
                double LeftPhase = CurrentTime - Math.Floor(CurrentTime);
                double RightPhase = 1.0 - LeftPhase;

                Xp_index = (int)CurrentTime; // Ptr to current input sample
                // Perform left-wing inner product
                v = FilterKit.lrsFilterUp(Imp, ImpD, Nwing, Interp, Xp_array, Xp_index++, LeftPhase, -1);
                // Perform right-wing inner product
                v += FilterKit.lrsFilterUp(Imp, ImpD, Nwing, Interp, Xp_array, Xp_index, RightPhase, 1);

                v *= LpScl; // Normalize for unity filter gain

                Yp_array[Yp_index++] = v; // Deposit output
                CurrentTime += dt; // Move to next sample by time increment
            }

            this.Time = CurrentTime;
            return Yp_index; // Return the number of output samples
        }

        private int lrsSrcUD(float[] X, float[] Y, double factor, int Nx, int Nwing, float LpScl, float[] Imp,
                             float[] ImpD, bool Interp)
        {

            float[] Xp_array = X;
            int Xp_index;

            float[] Yp_array = Y;
            int Yp_index = 0;

            float v;

            double CurrentTime = this.Time;
            double dh; // Step through filter impulse response
            double dt; // Step through input signal
            double endTime; // When Time reaches EndTime, return to user

            dt = 1.0 / factor; // Output sampling period

            dh = Math.Min(Npc, factor * Npc); // Filter sampling period

            endTime = CurrentTime + Nx;
            while (CurrentTime < endTime)
            {
                double LeftPhase = CurrentTime - Math.Floor(CurrentTime);
                double RightPhase = 1.0 - LeftPhase;

                Xp_index = (int)CurrentTime; // Ptr to current input sample
                // Perform left-wing inner product
                v = FilterKit.lrsFilterUD(Imp, ImpD, Nwing, Interp, Xp_array, Xp_index++, LeftPhase, -1, dh);
                // Perform right-wing inner product
                v += FilterKit.lrsFilterUD(Imp, ImpD, Nwing, Interp, Xp_array, Xp_index, RightPhase, 1, dh);

                v *= LpScl; // Normalize for unity filter gain

                Yp_array[Yp_index++] = v; // Deposit output

                CurrentTime += dt; // Move to next sample by time increment
            }

            this.Time = CurrentTime;
            return Yp_index; // Return the number of output samples
        }
    }

}