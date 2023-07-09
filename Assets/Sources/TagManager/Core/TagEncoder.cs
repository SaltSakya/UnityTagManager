using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SaltSakya.TagManager
{
    public class TagEncoder : MonoBehaviour
    {
        public string SavePath = "Sources/TagManager/Tags";
        public string TagName = "Tag";
        public ETagGeneratorDataType DataType = ETagGeneratorDataType.Uint32;

        public void Generate()
        {
            if (EncodeEnumIndex(
                    out var encodedEnums, 
                    out var minBits))
            {
                string type;
                switch (minBits)
                {
                    case ETagGeneratorDataType.Uint8:
                        type = "byte";
                        break;
                    case ETagGeneratorDataType.Uint16:
                        type = "ushort";
                        break;
                    case ETagGeneratorDataType.Uint32:
                        type = "uint";
                        break;
                    default:
                        type = "ulong";
                        break;
                }
                string fileContent = DictToEnum(TagName, type, ref encodedEnums);
                
                FileStream stream = new FileStream($"{Application.dataPath}/{SavePath}/{TagName}.cs", FileMode.OpenOrCreate);
                StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
                writer.Write(fileContent);
                writer.Flush();
                writer.Close();
                stream.Close();
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        
        #region Encoder
        struct SingleTagData
        {
            public Transform Transform;
            public ulong ParentValue;
            public int Index;
        }

        private bool EncodeEnumIndex(out Dictionary<string, ulong> result, out ETagGeneratorDataType type)
        {
            // 输出值
            result = new();
            type = DataType;
            
            // 用于实现 BFS 的容器
            Queue<SingleTagData> queue = new();
            
            // 记录每一层的最大子标签数 
            List<int> capacities = new();
            
            // 获取枚举类型
            CountOfMask(DataType, out int totalBitCount, out int dataBitCount);

            // 标签名的集合，用于检查是否有重复的标签。
            HashSet<string> tagNames = new();

            // 处理根节点
            capacities.Add(transform.childCount);
            for (int i = 0; i < transform.childCount; i++)
            {
                queue.Enqueue(new SingleTagData()
                {
                    Transform = transform.GetChild(i),
                    ParentValue = 0,
                    Index = i
                });
            }
            
            // Count of bits of the mask
            int bitsOfTotalMask = 0;

            // BFS
            while (queue.Count > 0)
            {
                // 子层级的最大子节点数 
                int childMaxCapacity = 0;
                
                // 当前层级的最大子节点数
                int bitsOfMask = CountOfBits(capacities[^1]);
                bitsOfTotalMask += bitsOfMask;
                
                if (bitsOfTotalMask > dataBitCount)
                {
                    Debug.LogError("[Error] Out Of Range. " +
                                   "This can be solved by using type with more bits (like ulong) " +
                                   "or splitting the first-level tags into several independent Tags.");
                    return false;
                }

                int count = queue.Count;
                for (int i = 0; i < count; i++)
                {
                    // Get current node which is a transform
                    SingleTagData data = queue.Dequeue();
                    
                    // If current Tag has existed, log error
                    if (tagNames.Contains(data.Transform.name))
                    {
                        Debug.LogError($"[Error] Duplicate Tag: {data.Transform.name}.");
                        return false;
                    }
                    else
                    {
                        tagNames.Add(data.Transform.name);
                    }

                    // Update the capacity of current depth
                    childMaxCapacity = Mathf.Max(childMaxCapacity, data.Transform.childCount);

                    // Value of current tag
                    ulong value = data.ParentValue +
                                  ((ulong)data.Index << (totalBitCount - bitsOfTotalMask)) +
                                  (ulong)bitsOfMask;
                    
                    // Add this Tag
                    result.Add(data.Transform.name, value);

                    // Enqueue current Transform's child Transforms
                    for (int j = 0; j < data.Transform.childCount;)
                        queue.Enqueue(new SingleTagData()
                        {
                            Transform = data.Transform.GetChild(j),
                            ParentValue = value,
                            Index = ++j
                        });
                }
                
                if (childMaxCapacity > 0)
                    capacities.Add(childMaxCapacity);
            }

            // 处理自动类型的值
            if (DataType == ETagGeneratorDataType.Auto)
            {
                // 用于存储右移次数的变量
                int rightMoveTimes = 0;
                ulong maskDivisor = 0;
                
                // 判断右移次数
                if (bitsOfTotalMask < 6)
                {
                    rightMoveTimes = 56;
                    maskDivisor = 8;
                    type = ETagGeneratorDataType.Uint8;
                }
                else if (bitsOfTotalMask < 13)
                {
                    rightMoveTimes = 48;
                    maskDivisor = 16;
                    type = ETagGeneratorDataType.Uint16;
                }
                else if (bitsOfTotalMask < 28)
                {
                    rightMoveTimes = 32;
                    maskDivisor = 32;
                    type = ETagGeneratorDataType.Uint32;
                }

                // 处理右移
                if (rightMoveTimes > 0)
                {
                    var tags = result.Keys.ToArray();
                    foreach (var tag in tags)
                    {
                        var value = result[tag];
                        result[tag] = (value >> rightMoveTimes) + (value % maskDivisor);
                    }
                }
            }
            
            return true;
        }


        #endregion
        
        private string DictToEnum(string name, string type, ref Dictionary<string, ulong> dictionary)
        {
            string content = "";
            foreach (var (e, i) in dictionary)
            {
                content += $"\n\t{e} = {i},";
            }

            return WrapFile(name, type, content);
        }

        private string WrapFile(string name, string type, string content)
        {
            return $"enum {name}: {type}\n{{{content}\n}};";
        }
        
        #region Tool functions

        private void CountOfMask(ETagGeneratorDataType type, out int totalBitCount, out int dataBitCount)
        {
            switch (type)
            {
                case ETagGeneratorDataType.Uint8:
                    totalBitCount = 8;
                    dataBitCount = 5;
                    break;
                case ETagGeneratorDataType.Uint16:
                    totalBitCount = 16;
                    dataBitCount = 12;
                    break;
                case ETagGeneratorDataType.Uint32:
                    totalBitCount = 32;
                    dataBitCount = 27;
                    break;
                default:
                    totalBitCount = 64;
                    dataBitCount = 58;
                    break;
            }
        }
        
        private int CountOfBits(int value)
        {
            int result = 0;
            while (value > 0)
            {
                result++;
                value /= 2;
            }
            
            return result;
        }

        private string ToBinary(ulong value, int bits)
        {
            string result = "";
            for (int i = 0; i < bits; i++)
            {
                result = (value % 2 > 0 ? "1" : "0") + result;
                value /= 2;
            }

            result = "0b" + result;
            return result;
        }
        #endregion
    }
    
    public enum ETagGeneratorDataType
    {
        Auto,
        Uint8,
        Uint16,
        Uint32,
        Uint64
    }
}
