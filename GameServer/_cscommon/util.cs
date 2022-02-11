using System;
using System.Collections.Generic;
using System.Text;

namespace WCS.Util
{
	public class UidGenerator
	{
		#region Singleton
		private static UidGenerator _instance = null;

		public static UidGenerator instance
		{
			get
			{
				if (null == _instance)
				{
					_instance = new UidGenerator();
				}

				return _instance;
			}
		}
		#endregion

		private long[] _uid;
		private int _counter = 0;

		public enum eUid
		{
			BASE_TIME = 1514732400,         // 기준 시간 "2018-01-01 00:00:00"
			user = 1,
			club = 2,
			room = 3,
			match = 4,
			item = 5,
            mail = 6,
            gm = 7,
			max = 8
		};

		public void Initialize(long server_index)
		{
			_uid = new long[(int)eUid.max];
			for (int i = 0; i < (int)eUid.max; i++)
			{
				_uid[i] = 0;
			}

			long now = (long)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

			// 기준 시간에서 현재 시간의 차이를 구한다.
			long diff = now - (long)eUid.BASE_TIME;

			string strDiff = string.Format("{0}", diff);
			string strDiffSeconds = string.Format("{0}{1}", new string('0', 9 - strDiff.Length), strDiff);

			// uid 생성 룰
			// server_index (10000 ~ 99999)            
			// 1234567890123456789

			// 구붑값 uid
			// server_index     구분값          시간차이(기준-현재)    나머지
			// 5                1               9                    4   
			// 12345            6               789012345            6789   

			// item uid
			// server_index     시간차이(기준-현재)     나머지
			// 5                9                       4   
			// 12345            678901234               56789   

			_uid[(int)(eUid.user)] = long.Parse(string.Format("{0}{1}{2}{3}", (int)(eUid.user), server_index, strDiffSeconds, "0000"));
			_uid[(int)(eUid.club)] = long.Parse(string.Format("{0}{1}{2}{3}", (int)(eUid.club), server_index, strDiffSeconds, "0000"));
			_uid[(int)(eUid.room)] = long.Parse(string.Format("{0}{1}{2}{3}", (int)(eUid.room), server_index, strDiffSeconds, "0000"));
			_uid[(int)(eUid.match)] = long.Parse(string.Format("{0}{1}{2}{3}", (int)(eUid.match), server_index, strDiffSeconds, "0000"));
			_uid[(int)(eUid.item)] = long.Parse(string.Format("{0}{1}{2}", server_index, strDiffSeconds, "00000"));
            _uid[(int)(eUid.mail)] = long.Parse(string.Format("{0}{1}{2}", server_index, strDiffSeconds, "00000"));
            _uid[(int)(eUid.gm)] = long.Parse(string.Format("{0}{1}{2}", server_index, strDiffSeconds, "00000"));
        }

		public long Get(eUid type)
		{
			//return System.Threading.Interlocked.Increment(ref _uid[(int)type]);

			switch (type)
			{
				case eUid.user:
				case eUid.club:
				case eUid.room:
				case eUid.match:
					_counter = System.Threading.Interlocked.Increment(ref _counter) % 10000;
					break;
				case eUid.item:
				case eUid.mail:
				case eUid.gm:
					_counter = System.Threading.Interlocked.Increment(ref _counter) % 100000;
					break;
			}

			return _uid[(int)type] + _counter;
		}
	}

	public class MidGenerator
	{
		#region Singleton
		private static MidGenerator _instance = null;

		public static MidGenerator instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new MidGenerator();
				}

				return _instance;
			}
		}
		#endregion

		private const int MAX_CHAR = 36;

		public string Get(byte counter)
		{
			DateTime now = DateTime.Now;

			int minute1;
			int minute2;
			if (now.Minute < 30)
			{
				minute1 = now.Minute;
				minute2 = (now.Minute + now.Second) % MAX_CHAR;
			}
			else
			{
				minute1 = ((now.Hour + now.Second) % 6) + 30;
				minute2 = now.Minute - 30;
			}

			int second1;
			int second2;
			if (now.Second < 30)
			{
				second1 = now.Second;
				second2 = 35 - counter;
			}
			else
			{
				second1 = ((now.Month + now.Second) % 6) + 30;
				second2 = now.Second - 30;
			}

			byte[] newId = new byte[9] {
					_GetFixedChar((byte)((minute2     + 10)   % MAX_CHAR)),
					_GetFixedChar((byte)((now.Month   + 23)   % MAX_CHAR)),
					_GetFixedChar((byte)((now.Hour    + 9)    % MAX_CHAR)),
					_GetFixedChar((byte)((second1     + 8)    % MAX_CHAR)),
					_GetFixedChar((byte)((now.Year    + 18)   % MAX_CHAR)),
					_GetFixedChar((byte)((minute1     + 6)    % MAX_CHAR)),
					_GetFixedChar((byte)((now.Day     + 7)    % MAX_CHAR)),
					_GetFixedChar((byte)((second2     + 21)   % MAX_CHAR)),
					_GetFixedChar(counter)
				};

			return Encoding.ASCII.GetString(newId);
		}

		private byte _GetFixedChar(byte number)
		{
			if (number < 26)
				return (byte)(65 + number);
			else
				return (byte)(48 + (number - 26));
		}
	}

	public class WcatRandom
	{
		#region Singleton
		private static WcatRandom _instance = null;

		public static WcatRandom instance
		{
			get
			{
				if (null == _instance)
				{
					_instance = new WcatRandom();
				}

				return _instance;
			}
		}
		#endregion

		private readonly Random _rand = null;

		public WcatRandom()
		{
			_rand = new Random();
		}

		public int Get()
		{
			return _rand.Next();
		}

		public int Get(int max)
		{
			// 0 ~ max - 1
			return _rand.Next(max);
		}

		public int Get(int min, int max)
		{
			// min ~ max - 1
			return _rand.Next(min, max);
		}

		public Guid GetGuid()
		{
			return Guid.NewGuid();
		}

        public void Shuffle<T>(List<T> list, int maxCount = 0)
        {
            List<T> tempList = new List<T>(list);
            list.Clear();

            if (tempList.Count < maxCount)
                maxCount = tempList.Count;

            for (int i = 0; i < maxCount; i++)
            {
                int index = _rand.Next(tempList.Count);
                T t = tempList[index];
                tempList.RemoveAt(index);
                list.Add(t);
            }
        }

		public void GetFenceIdx(List<int> list,int findbase, int maxCount, out List<byte> fence_idxs, out List<byte> fence_results)
		{
			WCS.logger.Warn($"GetFenceIdx list {list.Count} ");

			List<int> tempList = new List<int>(list);

			fence_idxs = new List<byte>();
			fence_results = new List<byte>();
			int loop = 0;
			for (int i = 0; i < maxCount;)
			{
				int index = _rand.Next(tempList.Count - 1);

				WCS.logger.Warn($"GetFenceIdx tempList {tempList.Count} index {index} ");
				if (tempList[index] == findbase)
				{
					fence_idxs.Add((byte)index);
					fence_results.Add((byte)tempList[index]);
					i++;
				}
				loop++;
				if (loop > 100)
					break;
			}
		}
		public void GetFenceIdx(List<int> list, int maxCount, out List<byte> fence_idxs,out List<byte> fence_results)
		{
			WCS.logger.Warn($"GetFenceIdx list {list.Count} ");

			List<int> tempList = new List<int>(list);

			fence_idxs = new List<byte>();
			fence_results = new List<byte>();
			int loop = 0;
			for (int i = 0; i < maxCount;)
			{
				int index = _rand.Next(tempList.Count -1);

				WCS.logger.Warn($"GetFenceIdx tempList {tempList.Count} index {index} ");
				if (tempList[index] != 0)
				{
					fence_idxs.Add((byte)index);
					fence_results.Add((byte)tempList[index]);
					i++;
				}
				loop++;
				if (loop > 100)
					break;
			}
		}

		public void GetFenceAll(List<int> list, out List<byte> fence_idxs, out List<byte> fence_results)
		{
			WCS.logger.Warn($"GetFenceIdx list {list.Count} ");

			List<int> tempList = new List<int>(list);

			fence_idxs = new List<byte>();
			fence_results = new List<byte>();
			for (int i = 0; i < 16;)
			{
				//int index = _rand.Next(tempList.Count - 1);

				
				
				{
					fence_idxs.Add((byte)i);
					fence_results.Add((byte)tempList[i]);
					i++;
				}
				
			}
		}
	}

    public static class DateTimeExtension
    {
        private readonly static DateTime _BASE_TIME = new DateTime(1970, 1, 1, 0, 0, 0);

        public static long ToUnixTimeLong(this DateTime time)
        {
            return (long)(time - _BASE_TIME).TotalSeconds;
        }

        public static int ToUnixTimeInt(this DateTime time)
        {
            return (int)(time - _BASE_TIME).TotalSeconds;
        }

        public static DateTime ToDateTime(int seconds)
        {
            return _BASE_TIME.AddSeconds(seconds);
        }

        public static int ToMakeDailyResetTime(this DateTime time, int hour, int min, int sec)
        {
            time = new DateTime(time.Year, time.Month, time.Day, hour, min, sec);

            return (int)(time - _BASE_TIME).TotalSeconds;
        }

        public static int GetUtcInterval()
        {
            return (int)(DateTime.UtcNow - DateTime.Now).TotalSeconds;
        }

        public static DateTime ToDate(string date)
        {
            return DateTime.ParseExact(date, "yyyyMMdd", null);            
        }

        public static DateTime ConvertDayOfWeekToDateTime(DateTime date, int dayofweek)
        {
            return date.AddDays(Convert.ToInt32(dayofweek) - Convert.ToInt32(date.DayOfWeek));
        }

        public static bool SameWeek(DateTime date_1, DateTime date_2)
        {
            var _date_1 = date_1.AddDays(Convert.ToInt32(DayOfWeek.Monday) - Convert.ToInt32(date_1.DayOfWeek));
            var _date_2 = date_2.AddDays(Convert.ToInt32(DayOfWeek.Monday) - Convert.ToInt32(date_2.DayOfWeek));

            return (_date_1.Day == _date_2.Day);
        }

        public static bool Between(DateTime in_date, DateTime date_1, DateTime date_2)
        {
            return (date_1 < in_date && date_2 > in_date);
        }

        public static int DifferenceDay(DateTime date_1, DateTime date_2) 
        {          
            TimeSpan TS = date_1 - date_2;
            return TS.Days;
        }
    }

    public static class Util
    {
        public static bool IsLinuxPlatform()
        {
            return Environment.OSVersion.Platform.ToString().Equals("unix", StringComparison.OrdinalIgnoreCase);
		}

		public static string GetServerSubPath(string conf_path, string server)
        {
            return $"{conf_path}{System.IO.Path.DirectorySeparatorChar}{server}.json";
        }

		public static int GetTickCount64()
		{
			return Environment.TickCount & Int32.MaxValue;
		}


		public static int GetCurrencies(List<WCS.Network.wcs_currency> currencies, int type)
        {
			return currencies.Find(i => i.tid == (int)type).count;
		}
	}
}