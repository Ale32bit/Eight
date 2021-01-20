using System;
using System.Collections.Generic;
using System.IO;

namespace Eight {
    public class EBF {
        public bool[][,] CharList;

        private byte[] _buffer;
        private int _i = 0;

        public byte Width;
        public byte Height;

        public static byte Version = 1;
        public EBF(string path) {
            _buffer = File.ReadAllBytes(path);

            if ( NextByte() != 0 || ReadString(3) != "EBF" ) {
                throw new Exception("Invalid EBF file");
            }

            if ( NextByte() != Version ) {
                throw new Exception("Unsupported version");
            }

            Width = NextByte();
            Height = NextByte();

            Dictionary<ushort, bool[,]> list = new();
            ushort highestPoint = 0;

            while ( _i < _buffer.Length ) {
                var point = NextUShort();
                highestPoint = Math.Max(highestPoint, point);

                var pW = NextByte();

                if ( pW > 8 ) {
                    throw new Exception("Width out of bounds");
                }

                var pH = NextByte();

                var matrix = new bool[pH, pW];
                for ( int y = 0; y < pH; y++ ) {
                    var r = NextByte();
                    for ( int x = pW; x > 0; x-- ) {
                        int bit = r & 1;
                        r >>= 1;
                        matrix[y, x - 1] = bit == 1;
                    }
                }

                list[point] = matrix;
            }

            CharList = new bool[highestPoint + 1][,];

            foreach ( var p in list.Keys ) {
                CharList[p] = list[p];
            }
        }

        private byte NextByte() {
            var b = _buffer[_i];
            _i++;
            return b;
        }

        private ushort NextUShort() {
            var b = _buffer[_i];
            ushort s = b;
            s <<= 8;
            s = (ushort)(s | _buffer[_i + 1]);

            _i += 2;
            return s;
        }

        private byte[] Read(int l) {
            byte[] arr = new byte[l];

            for ( int i = 0; i < l; i++ ) {
                arr[i] = _buffer[_i];
                _i++;
            }

            return arr;
        }

        private string ReadString(int l) {
            char[] arr = new char[l];

            for ( int i = 0; i < l; i++ ) {
                arr[i] = (char)_buffer[_i];
                _i++;
            }

            return new string(arr);
        }
    }
}
