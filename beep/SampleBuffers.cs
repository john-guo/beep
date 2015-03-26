using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beep
{
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

    /**
     * Callback for producing and consuming samples. Enables on-the-fly conversion between sample types
     * (signed 16-bit integers to floats, for example) and/or writing directly to an output stream.
     */
    public class SampleBuffers
    {
        /**
         * @return number of input samples available
         */

        private float[] _inBuffers;
        private float[] _outBuffers;

        private int _inIndex;
        private int _outIndex;

        private int _inLen;
        private int _outLen;

        public SampleBuffers(float[] inBuffers, int inOffset, int inLen, float[] outBuffers, int outOffset, int outLen)
        {
            _inBuffers = inBuffers;
            _outBuffers = outBuffers;

            _inIndex = inOffset;
            _outIndex = outOffset;

            _inLen = inLen;
            _outLen = outLen;
        }

        public int getInputBufferLength()
        {
            return _inLen - _inIndex;
        }

        /**
         * @return number of samples the output buffer has room for
         */
        public int getOutputBufferLength()
        {
            return _outLen - _outIndex;
        }

        /**
         * Copy <code>length</code> samples from the input buffer to the given array, starting at the given offset.
         * Samples should be in the range -1.0f to 1.0f.
         *
         * @param array  array to hold samples from the input buffer
         * @param offset start writing samples here
         * @param length write this many samples
         */
        public void produceInput(float[] array, int offset, int length)
        {
            for (int i = offset; i < offset + length; ++i)
            {
                array[i] = _inBuffers[_inIndex++];
            }
        }

        /**
         * Copy <code>length</code> samples from the given array to the output buffer, starting at the given offset.
         *
         * @param array  array to read from
         * @param offset start reading samples here
         * @param length read this many samples
         */
        public void consumeOutput(float[] array, int offset, int length)
        {
            for (int i = offset; i < offset + length; ++i)
            {
                _outBuffers[_outIndex++] = array[i];
            }
        }

        public int GetOutCount()
        {
            return _outIndex;
        }
    }

}
