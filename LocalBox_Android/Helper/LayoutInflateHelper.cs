using System;

using Android.Content;
using Android.Runtime;
using Android.Views;

namespace localbox.android
{
	public static class LayoutInflateHelper
	{
		/// <summary>
		///   Will obtain an instance of a LayoutInflater for the specified Context.
		/// </summary>
		/// <param name="context"> </param>
		/// <returns> </returns>
		public static LayoutInflater GetLayoutInflater(this Context context)
		{
			return context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
		}

	}
}

