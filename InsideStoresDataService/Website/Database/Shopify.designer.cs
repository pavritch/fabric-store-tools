﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Website
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="Shopify")]
	public partial class ShopifyDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertLog(Website.Shopify.Entities.Log instance);
    partial void UpdateLog(Website.Shopify.Entities.Log instance);
    partial void DeleteLog(Website.Shopify.Entities.Log instance);
    partial void InsertLiveShopifyProduct(Website.Shopify.Entities.LiveShopifyProduct instance);
    partial void UpdateLiveShopifyProduct(Website.Shopify.Entities.LiveShopifyProduct instance);
    partial void DeleteLiveShopifyProduct(Website.Shopify.Entities.LiveShopifyProduct instance);
    partial void InsertProductEvent(Website.Shopify.Entities.ProductEvent instance);
    partial void UpdateProductEvent(Website.Shopify.Entities.ProductEvent instance);
    partial void DeleteProductEvent(Website.Shopify.Entities.ProductEvent instance);
    partial void InsertProduct(Website.Shopify.Entities.Product instance);
    partial void UpdateProduct(Website.Shopify.Entities.Product instance);
    partial void DeleteProduct(Website.Shopify.Entities.Product instance);
    #endregion
		
		public ShopifyDataContext() : 
				base(global::System.Configuration.ConfigurationManager.ConnectionStrings["ShopifyConnectionString"].ConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public ShopifyDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ShopifyDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ShopifyDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ShopifyDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<Website.Shopify.Entities.Log> Logs
		{
			get
			{
				return this.GetTable<Website.Shopify.Entities.Log>();
			}
		}
		
		public System.Data.Linq.Table<Website.Shopify.Entities.LiveShopifyProduct> LiveShopifyProducts
		{
			get
			{
				return this.GetTable<Website.Shopify.Entities.LiveShopifyProduct>();
			}
		}
		
		public System.Data.Linq.Table<Website.Shopify.Entities.ProductEvent> ProductEvents
		{
			get
			{
				return this.GetTable<Website.Shopify.Entities.ProductEvent>();
			}
		}
		
		public System.Data.Linq.Table<Website.Shopify.Entities.Product> Products
		{
			get
			{
				return this.GetTable<Website.Shopify.Entities.Product>();
			}
		}
	}
}
namespace Website.Shopify.Entities
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.[Log]")]
	public partial class Log : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _RecordID;
		
		private int _Locus;
		
		private int _Type;
		
		private string _Message;
		
		private string _Data;
		
		private System.DateTime _Created;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnRecordIDChanging(int value);
    partial void OnRecordIDChanged();
    partial void OnLocusChanging(int value);
    partial void OnLocusChanged();
    partial void OnTypeChanging(int value);
    partial void OnTypeChanged();
    partial void OnMessageChanging(string value);
    partial void OnMessageChanged();
    partial void OnDataChanging(string value);
    partial void OnDataChanged();
    partial void OnCreatedChanging(System.DateTime value);
    partial void OnCreatedChanged();
    #endregion
		
		public Log()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_RecordID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				if ((this._RecordID != value))
				{
					this.OnRecordIDChanging(value);
					this.SendPropertyChanging();
					this._RecordID = value;
					this.SendPropertyChanged("RecordID");
					this.OnRecordIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Locus", DbType="Int NOT NULL")]
		public int Locus
		{
			get
			{
				return this._Locus;
			}
			set
			{
				if ((this._Locus != value))
				{
					this.OnLocusChanging(value);
					this.SendPropertyChanging();
					this._Locus = value;
					this.SendPropertyChanged("Locus");
					this.OnLocusChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Type", DbType="Int NOT NULL")]
		public int Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				if ((this._Type != value))
				{
					this.OnTypeChanging(value);
					this.SendPropertyChanging();
					this._Type = value;
					this.SendPropertyChanged("Type");
					this.OnTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Message", DbType="NVarChar(1024) NOT NULL", CanBeNull=false)]
		public string Message
		{
			get
			{
				return this._Message;
			}
			set
			{
				if ((this._Message != value))
				{
					this.OnMessageChanging(value);
					this.SendPropertyChanging();
					this._Message = value;
					this.SendPropertyChanged("Message");
					this.OnMessageChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Data", DbType="NVarChar(MAX)")]
		public string Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				if ((this._Data != value))
				{
					this.OnDataChanging(value);
					this.SendPropertyChanging();
					this._Data = value;
					this.SendPropertyChanged("Data");
					this.OnDataChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Created", DbType="DateTime NOT NULL")]
		public System.DateTime Created
		{
			get
			{
				return this._Created;
			}
			set
			{
				if ((this._Created != value))
				{
					this.OnCreatedChanging(value);
					this.SendPropertyChanging();
					this._Created = value;
					this.SendPropertyChanged("Created");
					this.OnCreatedChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.LiveShopifyProducts")]
	public partial class LiveShopifyProduct : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _ShopifyProductID;
		
		private string _ShopifyHandle;
		
		private int _StoreID;
		
		private int _ProductID;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnShopifyProductIDChanging(long value);
    partial void OnShopifyProductIDChanged();
    partial void OnShopifyHandleChanging(string value);
    partial void OnShopifyHandleChanged();
    partial void OnStoreIDChanging(int value);
    partial void OnStoreIDChanged();
    partial void OnProductIDChanging(int value);
    partial void OnProductIDChanged();
    #endregion
		
		public LiveShopifyProduct()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ShopifyProductID", DbType="BigInt NOT NULL", IsPrimaryKey=true)]
		public long ShopifyProductID
		{
			get
			{
				return this._ShopifyProductID;
			}
			set
			{
				if ((this._ShopifyProductID != value))
				{
					this.OnShopifyProductIDChanging(value);
					this.SendPropertyChanging();
					this._ShopifyProductID = value;
					this.SendPropertyChanged("ShopifyProductID");
					this.OnShopifyProductIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ShopifyHandle", DbType="NVarChar(1024) NOT NULL", CanBeNull=false)]
		public string ShopifyHandle
		{
			get
			{
				return this._ShopifyHandle;
			}
			set
			{
				if ((this._ShopifyHandle != value))
				{
					this.OnShopifyHandleChanging(value);
					this.SendPropertyChanging();
					this._ShopifyHandle = value;
					this.SendPropertyChanged("ShopifyHandle");
					this.OnShopifyHandleChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StoreID", DbType="Int NOT NULL")]
		public int StoreID
		{
			get
			{
				return this._StoreID;
			}
			set
			{
				if ((this._StoreID != value))
				{
					this.OnStoreIDChanging(value);
					this.SendPropertyChanging();
					this._StoreID = value;
					this.SendPropertyChanged("StoreID");
					this.OnStoreIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ProductID", DbType="Int NOT NULL")]
		public int ProductID
		{
			get
			{
				return this._ProductID;
			}
			set
			{
				if ((this._ProductID != value))
				{
					this.OnProductIDChanging(value);
					this.SendPropertyChanging();
					this._ProductID = value;
					this.SendPropertyChanged("ProductID");
					this.OnProductIDChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ProductEvents")]
	public partial class ProductEvent : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _EventID;
		
		private int _StoreID;
		
		private int _ProductID;
		
		private int _Operation;
		
		private string _Data;
		
		private System.DateTime _Created;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnEventIDChanging(int value);
    partial void OnEventIDChanged();
    partial void OnStoreIDChanging(int value);
    partial void OnStoreIDChanged();
    partial void OnProductIDChanging(int value);
    partial void OnProductIDChanged();
    partial void OnOperationChanging(int value);
    partial void OnOperationChanged();
    partial void OnDataChanging(string value);
    partial void OnDataChanged();
    partial void OnCreatedChanging(System.DateTime value);
    partial void OnCreatedChanged();
    #endregion
		
		public ProductEvent()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_EventID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int EventID
		{
			get
			{
				return this._EventID;
			}
			set
			{
				if ((this._EventID != value))
				{
					this.OnEventIDChanging(value);
					this.SendPropertyChanging();
					this._EventID = value;
					this.SendPropertyChanged("EventID");
					this.OnEventIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StoreID", DbType="Int NOT NULL")]
		public int StoreID
		{
			get
			{
				return this._StoreID;
			}
			set
			{
				if ((this._StoreID != value))
				{
					this.OnStoreIDChanging(value);
					this.SendPropertyChanging();
					this._StoreID = value;
					this.SendPropertyChanged("StoreID");
					this.OnStoreIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ProductID", DbType="Int NOT NULL")]
		public int ProductID
		{
			get
			{
				return this._ProductID;
			}
			set
			{
				if ((this._ProductID != value))
				{
					this.OnProductIDChanging(value);
					this.SendPropertyChanging();
					this._ProductID = value;
					this.SendPropertyChanged("ProductID");
					this.OnProductIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Operation", DbType="Int NOT NULL")]
		public int Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				if ((this._Operation != value))
				{
					this.OnOperationChanging(value);
					this.SendPropertyChanging();
					this._Operation = value;
					this.SendPropertyChanged("Operation");
					this.OnOperationChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Data", DbType="NVarChar(MAX)")]
		public string Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				if ((this._Data != value))
				{
					this.OnDataChanging(value);
					this.SendPropertyChanging();
					this._Data = value;
					this.SendPropertyChanged("Data");
					this.OnDataChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Created", DbType="DateTime NOT NULL")]
		public System.DateTime Created
		{
			get
			{
				return this._Created;
			}
			set
			{
				if ((this._Created != value))
				{
					this.OnCreatedChanging(value);
					this.SendPropertyChanging();
					this._Created = value;
					this.SendPropertyChanged("Created");
					this.OnCreatedChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Products")]
	public partial class Product : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _PkID;
		
		private int _StoreID;
		
		private int _ProductID;
		
		private long _ShopifyProductID;
		
		private string _Data;
		
		private int _Status;
		
		private System.Nullable<int> _DisqualifiedReason;
		
		private System.DateTime _Created;
		
		private System.DateTime _LastModified;
		
		private System.Nullable<System.DateTime> _LastFullUpdate;
		
		private System.Nullable<System.DateTime> _LastAvailabilityUpdate;
		
		private System.Nullable<System.DateTime> _CreatedOnShopify;
		
		private System.Nullable<int> _LastOperation;
		
		private System.Nullable<int> _LastOperationResult;
		
		private string _LastOperationErrorData;
		
		private int _FullUpdateRequired;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnPkIDChanging(int value);
    partial void OnPkIDChanged();
    partial void OnStoreIDChanging(int value);
    partial void OnStoreIDChanged();
    partial void OnProductIDChanging(int value);
    partial void OnProductIDChanged();
    partial void OnShopifyProductIDChanging(long value);
    partial void OnShopifyProductIDChanged();
    partial void OnDataChanging(string value);
    partial void OnDataChanged();
    partial void OnStatusChanging(int value);
    partial void OnStatusChanged();
    partial void OnDisqualifiedReasonChanging(System.Nullable<int> value);
    partial void OnDisqualifiedReasonChanged();
    partial void OnCreatedChanging(System.DateTime value);
    partial void OnCreatedChanged();
    partial void OnLastModifiedChanging(System.DateTime value);
    partial void OnLastModifiedChanged();
    partial void OnLastFullUpdateChanging(System.Nullable<System.DateTime> value);
    partial void OnLastFullUpdateChanged();
    partial void OnLastAvailabilityUpdateChanging(System.Nullable<System.DateTime> value);
    partial void OnLastAvailabilityUpdateChanged();
    partial void OnCreatedOnShopifyChanging(System.Nullable<System.DateTime> value);
    partial void OnCreatedOnShopifyChanged();
    partial void OnLastOperationChanging(System.Nullable<int> value);
    partial void OnLastOperationChanged();
    partial void OnLastOperationResultChanging(System.Nullable<int> value);
    partial void OnLastOperationResultChanged();
    partial void OnLastOperationErrorDataChanging(string value);
    partial void OnLastOperationErrorDataChanged();
    partial void OnFullUpdateRequiredChanging(int value);
    partial void OnFullUpdateRequiredChanged();
    #endregion
		
		public Product()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PkID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int PkID
		{
			get
			{
				return this._PkID;
			}
			set
			{
				if ((this._PkID != value))
				{
					this.OnPkIDChanging(value);
					this.SendPropertyChanging();
					this._PkID = value;
					this.SendPropertyChanged("PkID");
					this.OnPkIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StoreID", DbType="Int NOT NULL")]
		public int StoreID
		{
			get
			{
				return this._StoreID;
			}
			set
			{
				if ((this._StoreID != value))
				{
					this.OnStoreIDChanging(value);
					this.SendPropertyChanging();
					this._StoreID = value;
					this.SendPropertyChanged("StoreID");
					this.OnStoreIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ProductID", DbType="Int NOT NULL")]
		public int ProductID
		{
			get
			{
				return this._ProductID;
			}
			set
			{
				if ((this._ProductID != value))
				{
					this.OnProductIDChanging(value);
					this.SendPropertyChanging();
					this._ProductID = value;
					this.SendPropertyChanged("ProductID");
					this.OnProductIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ShopifyProductID", DbType="BigInt NOT NULL")]
		public long ShopifyProductID
		{
			get
			{
				return this._ShopifyProductID;
			}
			set
			{
				if ((this._ShopifyProductID != value))
				{
					this.OnShopifyProductIDChanging(value);
					this.SendPropertyChanging();
					this._ShopifyProductID = value;
					this.SendPropertyChanged("ShopifyProductID");
					this.OnShopifyProductIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Data", DbType="NVarChar(MAX) NOT NULL", CanBeNull=false)]
		public string Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				if ((this._Data != value))
				{
					this.OnDataChanging(value);
					this.SendPropertyChanging();
					this._Data = value;
					this.SendPropertyChanged("Data");
					this.OnDataChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Status", DbType="Int NOT NULL")]
		public int Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				if ((this._Status != value))
				{
					this.OnStatusChanging(value);
					this.SendPropertyChanging();
					this._Status = value;
					this.SendPropertyChanged("Status");
					this.OnStatusChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DisqualifiedReason", DbType="Int")]
		public System.Nullable<int> DisqualifiedReason
		{
			get
			{
				return this._DisqualifiedReason;
			}
			set
			{
				if ((this._DisqualifiedReason != value))
				{
					this.OnDisqualifiedReasonChanging(value);
					this.SendPropertyChanging();
					this._DisqualifiedReason = value;
					this.SendPropertyChanged("DisqualifiedReason");
					this.OnDisqualifiedReasonChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Created", DbType="DateTime NOT NULL")]
		public System.DateTime Created
		{
			get
			{
				return this._Created;
			}
			set
			{
				if ((this._Created != value))
				{
					this.OnCreatedChanging(value);
					this.SendPropertyChanging();
					this._Created = value;
					this.SendPropertyChanged("Created");
					this.OnCreatedChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastModified", DbType="DateTime NOT NULL")]
		public System.DateTime LastModified
		{
			get
			{
				return this._LastModified;
			}
			set
			{
				if ((this._LastModified != value))
				{
					this.OnLastModifiedChanging(value);
					this.SendPropertyChanging();
					this._LastModified = value;
					this.SendPropertyChanged("LastModified");
					this.OnLastModifiedChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastFullUpdate", DbType="DateTime")]
		public System.Nullable<System.DateTime> LastFullUpdate
		{
			get
			{
				return this._LastFullUpdate;
			}
			set
			{
				if ((this._LastFullUpdate != value))
				{
					this.OnLastFullUpdateChanging(value);
					this.SendPropertyChanging();
					this._LastFullUpdate = value;
					this.SendPropertyChanged("LastFullUpdate");
					this.OnLastFullUpdateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastAvailabilityUpdate", DbType="DateTime")]
		public System.Nullable<System.DateTime> LastAvailabilityUpdate
		{
			get
			{
				return this._LastAvailabilityUpdate;
			}
			set
			{
				if ((this._LastAvailabilityUpdate != value))
				{
					this.OnLastAvailabilityUpdateChanging(value);
					this.SendPropertyChanging();
					this._LastAvailabilityUpdate = value;
					this.SendPropertyChanged("LastAvailabilityUpdate");
					this.OnLastAvailabilityUpdateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CreatedOnShopify", DbType="DateTime")]
		public System.Nullable<System.DateTime> CreatedOnShopify
		{
			get
			{
				return this._CreatedOnShopify;
			}
			set
			{
				if ((this._CreatedOnShopify != value))
				{
					this.OnCreatedOnShopifyChanging(value);
					this.SendPropertyChanging();
					this._CreatedOnShopify = value;
					this.SendPropertyChanged("CreatedOnShopify");
					this.OnCreatedOnShopifyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastOperation", DbType="Int")]
		public System.Nullable<int> LastOperation
		{
			get
			{
				return this._LastOperation;
			}
			set
			{
				if ((this._LastOperation != value))
				{
					this.OnLastOperationChanging(value);
					this.SendPropertyChanging();
					this._LastOperation = value;
					this.SendPropertyChanged("LastOperation");
					this.OnLastOperationChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastOperationResult", DbType="Int")]
		public System.Nullable<int> LastOperationResult
		{
			get
			{
				return this._LastOperationResult;
			}
			set
			{
				if ((this._LastOperationResult != value))
				{
					this.OnLastOperationResultChanging(value);
					this.SendPropertyChanging();
					this._LastOperationResult = value;
					this.SendPropertyChanged("LastOperationResult");
					this.OnLastOperationResultChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastOperationErrorData", DbType="NVarChar(MAX)")]
		public string LastOperationErrorData
		{
			get
			{
				return this._LastOperationErrorData;
			}
			set
			{
				if ((this._LastOperationErrorData != value))
				{
					this.OnLastOperationErrorDataChanging(value);
					this.SendPropertyChanging();
					this._LastOperationErrorData = value;
					this.SendPropertyChanged("LastOperationErrorData");
					this.OnLastOperationErrorDataChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FullUpdateRequired", DbType="Int NOT NULL")]
		public int FullUpdateRequired
		{
			get
			{
				return this._FullUpdateRequired;
			}
			set
			{
				if ((this._FullUpdateRequired != value))
				{
					this.OnFullUpdateRequiredChanging(value);
					this.SendPropertyChanging();
					this._FullUpdateRequired = value;
					this.SendPropertyChanged("FullUpdateRequired");
					this.OnFullUpdateRequiredChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591