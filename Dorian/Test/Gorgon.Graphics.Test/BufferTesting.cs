﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Thursday, May 16, 2013 9:23:14 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Test.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GorgonLibrary.Graphics.Test
{
	/// <summary>
	/// BufferTesting
	/// </summary>
	[TestClass]
	public class BufferTesting
	{
		private GraphicsFramework _framework;
	    private GorgonDataStream _bufferStream;
		private string _tbShaders;
		private string _sbShaders;
	    private string _rbShaders;

		[TestInitialize]
		public void Init()
		{
			_framework = new GraphicsFramework();
			_tbShaders = Encoding.UTF8.GetString(Resources.TypedBufferShaders);
			_sbShaders = Encoding.UTF8.GetString(Resources.StructuredBufferShaders);
            _rbShaders = Encoding.UTF8.GetString(Resources.RawBufferShaders);
		}

		[TestCleanup]
		public void CleanUp()
		{
            if (_bufferStream != null)
            {
                _bufferStream.Dispose();
            }

			if (_framework != null)
			{
				_framework.Dispose();
			}
		}

		[TestMethod]
		public void BindStructBuffer()
		{
		    _framework.CreateTestScene(_sbShaders, _sbShaders, false);

			using (var buffer = _framework.Graphics.Shaders.CreateStructuredBuffer(new GorgonStructuredBufferSettings
			    {
					AllowCPUWrite = false,
					ElementCount = 4,
					ElementSize = 12,
					IsOutput = false,
					StructuredBufferType = StructuredBufferType.Standard
				}))
			{
				_bufferStream = new GorgonDataStream(48);

				_framework.Graphics.Shaders.VertexShader.Resources.SetShaderBuffer(0, buffer);
					
				_framework.MaxTimeout = 10000;

				_framework.IdleFunc = () =>
					{
						for (int i = 0; i < 4; i++)
						{
							var rnd = new Vector3(GorgonRandom.RandomSingle() * GorgonTiming.Delta,
								                        GorgonRandom.RandomSingle() * GorgonTiming.Delta,
								                        GorgonRandom.RandomSingle() * GorgonTiming.Delta);

                            _bufferStream.Write(rnd);
						}
                            
                        _bufferStream.Position = 0;
                        // ReSharper disable AccessToDisposedClosure
                        buffer.Update(_bufferStream);
                        // ReSharper restore AccessToDisposedClosure
					}; 

				_framework.Run();
			}
		}

		[TestMethod]
		public void BindTypedBuffer()
		{
			_framework.CreateTestScene(_tbShaders, _tbShaders, true);

			var values = new Vector4[256];

			for (int i = 0; i < values.Length; i++)
			{
				values[i] = new Vector4(GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle(), 1.0f);
			}

			using(var buffer = _framework.Graphics.Shaders.CreateTypedBuffer(values, BufferFormat.R32G32B32A32_Float, false))
			{
				_framework.Graphics.Shaders.PixelShader.Resources.SetShaderBuffer(0, buffer);

				Assert.IsTrue(_framework.Run() == DialogResult.Yes);
			}
			
		}

        [TestMethod]
        public void BindRawBuffer()
        {
            _framework.CreateTestScene(_rbShaders, _rbShaders, true);

            var values = new byte[256 * 4];

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (byte)GorgonRandom.RandomInt32(256);
            }

            using(var stream = GorgonDataStream.ArrayToStream(values))
            {
                using(var buffer = _framework.Graphics.Shaders.CreateTypedBuffer<Vector4>(new GorgonTypedBufferSettings
                    {
                        AllowCPUWrite = false,
                        ElementCount = 256,
                        IsRaw = true,
                        ShaderViewFormat = BufferFormat.R32_UInt
                    }, stream))
                {
                    _framework.Graphics.Shaders.PixelShader.Resources.SetShaderBuffer(0, buffer);

                    Assert.IsTrue(_framework.Run() == DialogResult.Yes);
                }
            }

        }
    }
}