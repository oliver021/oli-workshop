using System;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Net;

namespace OliWorkshop.Serializer.Blobs
{
    /// <summary>
    /// The basic delegate to represent the setter method
    /// </summary>
    /// <param name="value"></param>
    public delegate void SetterValue(object value);

    /// <summary>
    /// Main class that contains the basic method to converto from object to blob bytes 
    /// and the invert action
    /// Use <see cref="SerializeObject(object, SerializerOptions)"/> to convert a object to binary blob
    /// and <see cref="DeserializeObject{T}(byte[], SerializerOptions)"/> to invert process
    /// </summary>
    public static class BlobConvert
    {
        /// <summary>
        /// Serialize an object to return the array bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static byte[] SerializeObject(object data, SerializerOptions options)
        {
            // the options should be set
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // get the reflection object
            var reflect = data.GetType();

            // create a new blob tracker array to set the result
            var blob = new ArrayTracker(1024);

            // evaluate of fields
            if (options.InFields)
            {
                foreach ( var item in reflect.GetFields().Where(f => !f.IsDefined(typeof(NonSerializedAttribute))) )
                {
                    WriteBuffer(item.GetValue(data), item.FieldType, blob, options);
                }
            }

            // evaluate the properties
            if (options.InProperties)
            {
                foreach ( var item in reflect.GetProperties().Where(f => !f.IsDefined(typeof(NonSerializedAttribute))) )
                {
                    WriteBuffer(item.GetValue(data), item.PropertyType, blob, options);
                }
            }

            // resize the blob buffer base on record
            Array.Resize(ref blob.buffer, blob.record);

            // return the final result
            return blob.buffer;
        }

        /// <summary>
        /// Extract the instace with values from blobs array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(byte[] data, SerializerOptions options) where T : new()
        {
            var result = new T();
            Populate(result,data,options);
            return result;
        }

        /// <summary>
        /// Extract the instace with values from blobs array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static void Populate(object target, byte[] data, SerializerOptions options)
        {
            // basic initializations
            var reflect = target.GetType();
            var counter = 0;

            // scan values to fields
            if (options.InFields)
            {
                foreach (var item in reflect.GetFields().Where(f => !f.IsDefined(typeof(NonSerializedAttribute))))
                {
                    ReadFromBlob(item.FieldType, x => item.SetValue(target, x), data, ref counter);
                }
            }

            // scan values to properties
            if (options.InProperties)
            {
                foreach (var item in reflect.GetProperties().Where(f => !f.IsDefined(typeof(NonSerializedAttribute))))
                {
                }
            }
        }

        /// <summary>
        /// Helper to read data from binary sequence to extract the .Net value type
        /// </summary>
        /// <param name="target"></param>
        /// <param name="reflect"></param>
        /// <param name="data"></param>
        /// <param name="counter"></param>
        private static void ReadFromBlob(Type reflect, SetterValue setter, byte[] data, ref int counter)
        {
            // byte code specification
            byte code = 0;

            // switch iteration control about different type value
            switch (reflect.Name)
            {
                case nameof(String):
                    code = data[counter];
                    
                    if (code != ByteCodes.String && code != ByteCodes.Parseable)
                    {
                        throw new InvalidOperationException();
                    }

                    counter++;
                    short length = BitConverter.ToInt16(data.Take(counter, 2), 0);

                    setter(Encoding.UTF8.GetString(data, counter+2, length));
                    counter += (2 + length);
                    break;

                case nameof(Boolean):
                    code = data[counter];
                    if (code != ByteCodes.Boolean)
                    {
                        throw new InvalidOperationException();
                    }
                    counter++;
                    bool valueBool = data[counter] == 1 ? true : false;
                    counter++;
                    setter(valueBool);
                    break;

                case nameof(DateTime):
                case nameof(Version):
                case nameof(IPAddress):
                case nameof(Guid):
                case nameof(TimeSpan):
                case nameof(Uri):
                    code = data[counter];

                    if (code == ByteCodes.Null)
                    {
                        counter++;
                        break;
                    }

                    if ( code != ByteCodes.Parseable)
                    {
                        throw new InvalidOperationException();
                    }

                    counter++;
                    byte length2 = data[counter];
                    counter++;
                    string valueParseable = Encoding.UTF8.GetString(data, counter, length2);
                    var delegFunc = reflect.GetMethod("Parse", new[] { typeof(string) });
                    setter(delegFunc.Invoke(null, new[] { valueParseable }));
                    counter += length2;
                    break;
            }
        }

        /// <summary>
        /// Write bytes from value reflection to blob's buffer
        /// </summary>
        /// <param name="value"></param>
        /// <param name="meta"></param>
        /// <param name="blob"></param>
        /// <param name="options"></param>
        private static void WriteBuffer(object value, Type meta, ArrayTracker blob, SerializerOptions options)
        {
            /// control above the current value to evaluate the bytes to serialize this item
            switch (value)
            {
                case string current:
                    var strArrBytes = Encoding.UTF8.GetBytes(current);
                    short length = (short)strArrBytes.Length;
                    blob.WriteBuffer(ByteCodes.String, BitConverter.GetBytes(length), strArrBytes);
                    break;

                 //// The numeric types representation for many aspects
                 /// like the byte length, if is unsigned and if is float, double or integer type
                case bool current:
                    byte boolVal = (byte) (current ? 1 : 0);
                    blob.WriteBuffer(ByteCodes.Boolean, new byte[1] {boolVal});
                    break;

                case ushort current:
                    blob.WriteBuffer(ByteCodes.NumberUTwoBytes, BitConverter.GetBytes(current));
                    break;

                case short current:
                    blob.WriteBuffer(ByteCodes.NumberTwoBytes, BitConverter.GetBytes(current));
                    break;

                case int current:
                    blob.WriteBuffer(ByteCodes.NumberFourBytes, BitConverter.GetBytes(current));
                    break;

                case uint current:
                    blob.WriteBuffer(ByteCodes.NumberUFourBytes, BitConverter.GetBytes(current));
                    break;

                case long current:
                    blob.WriteBuffer(ByteCodes.NumberEigthBytes, BitConverter.GetBytes(current));
                    break;

                case ulong current:
                    blob.WriteBuffer(ByteCodes.NumberUEigthBytes, BitConverter.GetBytes(current));
                    break;

                case double current:
                    blob.WriteBuffer(ByteCodes.NumberDouble, BitConverter.GetBytes(current));
                    break;

                case float current:
                    blob.WriteBuffer(ByteCodes.NumberDouble, BitConverter.GetBytes(current));
                    break;

                default:
                    if (value is null) {
                        /// write the byte null
                        blob.WriteOnce(ByteCodes.Null);
                    } else if (
                            value is IPAddress ||
                            value is TimeSpan ||
                            value is Uri ||
                            value is DateTime ||
                            value is Guid ||
                            value is Version
                            )
                    {
                        // working with parseable values
                        var strArrBytes2 = Encoding.UTF8.GetBytes(value.ToString());
                        byte length2 = (byte)strArrBytes2.Length;

                        // store the parseable data
                        blob.WriteBuffer(ByteCodes.Parseable, new[] { length2 }, strArrBytes2);
                    }
                    else if (meta.IsInterface)
                    {
                        
                    }
                    else if(meta.IsEnum)
                    {
                        
                    }else if (meta.IsArray) 
                    { 

                    } else if (meta.IsClass)
                    {
                        // recursive sys
                        blob.WriteBuffer(ByteCodes.Object, SerializeObject(value, options));
                    }
                    break;
            }
        }
    }
}
