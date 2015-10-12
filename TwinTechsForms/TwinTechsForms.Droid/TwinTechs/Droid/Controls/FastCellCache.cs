using System;
using TwinTechs.Controls;
using System.Collections.Generic;
using Android.Views;
using System.Reflection;
using Xamarin.Forms;


namespace TwinTechs.Droid.Controls
{
	/// <summary>
	/// Make this class available through your favourite IOC/service method
	/// </summary>
	/// 
	public class FastCellCache : IFastCellCache
	{
		static FastCellCache _instance;

		FastCellCache ()
		{
			_cachedDataByView = new Dictionary<Android.Views.View, CachedData> ();
		}

		public static FastCellCache Instance { 
			get { 
				if (_instance == null) {
					_instance = new FastCellCache ();
				}
				return _instance; 
			} 
		}

		Dictionary<Android.Views.View,CachedData> _cachedDataByView;

		public CachedData GetCellCache (ViewGroup parent)
		{
			if (!_cachedDataByView.ContainsKey (parent)) {
				_cachedDataByView [parent] = new CachedData ();
			}
			return _cachedDataByView [parent];
		}

		#region IFastCellCache impl

		//TODO maintain mapping of cell to original?

		public void FlushAllCaches ()
		{
			foreach (var cachedData in _cachedDataByView.Values) {
				cachedData.Reset ();
			}

			_cachedDataByView = new Dictionary<Android.Views.View, CachedData> ();
		}

		#endregion

		public class CachedData
		{
			internal CachedData ()
			{
				Reset ();
			}

			/// <summary>
			/// Reset this instance. 
			/// </summary>
			internal void Reset ()
			{
				CoreItemsByReuseIdentifiers = new Dictionary<string, Dictionary<Android.Views.View, FastCell>> ();
//				CellItemsByCoreCells = new Dictionary<Android.Views.View, FastCell> ();
				OriginalBindingContextsForReusedItems = new Dictionary<FastCell, object> ();
			}

			Dictionary<string, Dictionary<Android.Views.View, FastCell>> CoreItemsByReuseIdentifiers { get; set; }

//			Dictionary<Android.Views.View, FastCell> CellItemsByCoreCells { get; set; }

			Dictionary<FastCell,object> OriginalBindingContextsForReusedItems { get; set; }

			public void RecycleCell (Android.Views.View view, FastCell newCell)
			{
				var cellItemsByCoreCells = GetCellItemsByCoreCells (newCell.ReuseIdentifier);
				if (cellItemsByCoreCells.ContainsKey (view)) {
					var reusedItem = cellItemsByCoreCells [view];
					if (OriginalBindingContextsForReusedItems.ContainsKey (newCell)) {
						reusedItem.BindingContext = OriginalBindingContextsForReusedItems [newCell];
					} else {
						reusedItem.BindingContext = newCell.BindingContext;
					}
				}
			}

			public bool IsCached (Android.Views.View view, string reuseIdentifier)
			{
				var cellItemsByCoreCells = GetCellItemsByCoreCells (reuseIdentifier);
				return cellItemsByCoreCells.ContainsKey (view);
			}

			public void CacheCell (FastCell cell, Android.Views.View view)
			{
				var cellItemsByCoreCells = GetCellItemsByCoreCells (cell.ReuseIdentifier);
				cellItemsByCoreCells [view] = cell;

				OriginalBindingContextsForReusedItems [cell] = cell.BindingContext;
			}

			private Dictionary<Android.Views.View, FastCell> GetCellItemsByCoreCells(string reuseIdentifier){
				if (!CoreItemsByReuseIdentifiers.ContainsKey (reuseIdentifier)) {
					CoreItemsByReuseIdentifiers [reuseIdentifier] = new Dictionary<Android.Views.View, FastCell> ();
				}
				return CoreItemsByReuseIdentifiers [reuseIdentifier];
			}

//			public object GetBindingContextForReusedCell (FastCell cell)
//			{
//				if (OriginalBindingContextsForReusedItems.ContainsKey (cell)) {
//					return OriginalBindingContextsForReusedItems [cell];
//				} else {
//					return null;
//				}
//			}
//
//			void CacheBindingContextForReusedCell (FastCell cell)
//			{
//				OriginalBindingContextsForReusedItems [cell] = cell.BindingContext;
//			}




		}
	}
}

