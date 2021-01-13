using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HongliuSchool
{
    public class InfiniteNumber
    {
        // 超大数显示的单位，超过后使用"U"+UnitLevel显示，如7.18U120
        private static readonly string[] Unit = {"", "k", "M", "B", "C", "D", "E", "F", "G", "H"};
        // 用于存储位数的具体信息
        private readonly List<ushort> _digits = new List<ushort>();

        public InfiniteNumber()
        {
        }

        private InfiniteNumber(InfiniteNumber value, int reduceDigitCount = 0)
        {
            if (reduceDigitCount >= value._digits.Count) return;
            for (int i = 0; i < value._digits.Count - reduceDigitCount; i++)
            {
                _digits.Add(value._digits[i]);
            }
        }

        // 1923658
        public InfiniteNumber(long value)
        {
            while (value != 0)
            {
                var count = value % 1000;
                _digits.Add((ushort)count);
                value /= 1000;
            }
        }

        // 解析类似“7.18H”的数字
        public static InfiniteNumber Parse(string value)
        {
            var retNumber = new InfiniteNumber();
            // 用正则表达式验证字符串是否符合规则
            if (Regex.Matches(value, @"^[0-9]+\.?[0-9]{0,3}[0-9A-Z]*$").Count == 0)
            {
                throw new Exception("不符合数据格式");
            }

            var reg = Regex.Matches(value, "[A-Z]");
            // 找到第一个字母所在的位置
            if (reg.Count > 0) // 有字母单位
            {
                var subNumber = value.Substring(0, reg[0].Index);
                var subUnit = value.Substring(reg[0].Index, value.Length - reg[0].Index);

                ushort num1 = 0;
                ushort num2 = 0;
                // 数字部分解析
                if (subNumber.Contains('.')) //数字部分包含小数部分
                {
                    // 使用字符串形式解析而不直接使用double的原因在于double解析可能会引入精度误差
                    var dotIndex = subNumber.IndexOf('.');
                    var frontPart = subNumber.Substring(0, dotIndex);
                    var backPart = subNumber.Substring(dotIndex+1, subNumber.Length-dotIndex-1);
                    for (int i = backPart.Length; i < 3; i++)
                    {
                        backPart += "0";
                    }
                    
                    // 最后直接使用整数解析不会引入精度误差
                    num1 = ushort.Parse(frontPart);
                    num2 = ushort.Parse(backPart);
                }
                else
                {
                    num1 = ushort.Parse(subNumber);
                }
                
                // 单位部分解析
                var subUnitReg = Regex.Matches(subUnit, "[0-9]");
                int unitLevel = 0;
                if (subUnitReg.Count > 0) // 说明有U18这种形式
                {
                    // 去掉U，取出后面的数字
                    var levelStr = subUnit.Substring(1, subUnit.Length - 1);
                    unitLevel = int.Parse(levelStr);
                }
                else
                {
                    // 从单位数组中检索
                    unitLevel = Array.IndexOf(Unit, subUnit);
                    if (unitLevel == -1) throw new Exception("单位错误");
                }
                    
                // 填充数据
                for (int i = 0; i <= unitLevel; i++)
                {
                    retNumber._digits.Add(0);
                }

                retNumber._digits[unitLevel] = num1;
                retNumber._digits[unitLevel-1] = num2;
            }
            else // 没字母
            {
                // 不支持浮点数，直接截断，例如不支持15.348
                var d = double.Parse(value);
                return new InfiniteNumber((long) d);
            }

            return retNumber;
        }

        public override bool Equals(object o)
        {
            if (o is InfiniteNumber rhs)
                return _digits.SequenceEqual(rhs._digits);

            return false;
        }

        public static bool operator >(InfiniteNumber lhs, InfiniteNumber rhs)
        {
            var lHighestDigit = lhs._digits.Count;
            var rHighestDigit = rhs._digits.Count;

            if (lHighestDigit == rHighestDigit)
            {
                for (int i = lHighestDigit - 1; i >= 0; i--)
                {
                    if (lhs._digits[i] == rhs._digits[i]) continue;
                    return lhs._digits[i] > rhs._digits[i];
                }
            }

            return lHighestDigit > rHighestDigit;
        }

        public static bool operator <(InfiniteNumber lhs, InfiniteNumber rhs)
        {
            return rhs > lhs;
        }

        // 加法
        public static InfiniteNumber operator +(InfiniteNumber lhs, InfiniteNumber rhs)
        {
            var newNumber = new InfiniteNumber();
            var lHighestDigit = lhs._digits.Count;
            var rHighestDigit = rhs._digits.Count;
            var maxDigitCount = Mathf.Max(lHighestDigit, rHighestDigit);

            ushort increase = 0;
            for (int i = 0; i < maxDigitCount; i++)
            {
                ushort d = 0;
                if (lHighestDigit > i) d += lhs._digits[i];
                if (rHighestDigit > i) d += rhs._digits[i];

                d += increase;
                increase = 0;

                if (d > 999)
                {
                    increase = (ushort)(d / 1000);
                    d %= 1000;
                }
            
                newNumber._digits.Add(d);
            }

            if (increase > 0)
                newNumber._digits.Add(increase);

            return newNumber;
        }

        int FindHighestDigit()
        {
            return _digits.Count - 1;
        }

        public override string ToString()
        {
            var index = FindHighestDigit();
            switch (index)
            {
                case -1:
                    return "0";
                case 0:
                    return _digits[0].ToString();
                default:
                {
                    var number = _digits[index] + _digits[index - 1] / 1000f;
                    return number.ToString("F2") + UnitStr(index);
                }
            }
        }
        
        // 
        public string UnitStr(int index)
        {
            if (index >= Unit.Length) return "U" + index;
            return Unit[index];
        }

        public string DebugString()
        {
            string ret = "";
            for (int i = _digits.Count - 1; i >= 0; i--)
            {
                ret += _digits[i].ToString("000") + ",";
            }

            return ret;
        }
    }
}