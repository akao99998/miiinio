using System;
using System.Reflection;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public abstract class ReturnValueContainer
	{
		public enum ValueType
		{
			Void = 0,
			Nil = 1,
			Number = 2,
			Boolean = 3,
			String = 4,
			Dictionary = 5,
			Array = 6
		}

		protected readonly IKampaiLogger logger;

		private ValueType _type;

		protected bool boolValue;

		protected float numberValue;

		protected string stringValue;

		protected ValueType type
		{
			get
			{
				return _type;
			}
			set
			{
				if (_type != value)
				{
					if (_type == ValueType.Array)
					{
						ClearArray();
					}
					if (_type == ValueType.Dictionary)
					{
						ClearKeys();
					}
					_type = value;
				}
			}
		}

		public ReturnValueContainer(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		protected abstract ReturnValueContainer GetContainerForKey(string key);

		protected abstract ReturnValueContainer GetContainerForNextIndex();

		protected abstract void ClearKeys();

		protected abstract void ClearArray();

		public virtual void Reset()
		{
			SetVoid();
		}

		public void Set(object value)
		{
			if (value == null)
			{
				SetNil();
				return;
			}
			string name = value.GetType().Name;
			logger.Error("ReturnValueContainer: Setting value of type {0} is not supported!", name);
		}

		public void Set(IBoxed value)
		{
			Type type = value.GetType();
			MethodInfo getMethod = type.GetProperty("Value").GetGetMethod();
			object obj = getMethod.Invoke(value, new object[0]);
			MethodInfo method = typeof(ReturnValueContainer).GetMethod("Set", type.GetGenericArguments());
			method.Invoke(this, new object[1] { obj });
		}

		public void Set(ITuple value)
		{
			Type type = value.GetType();
			Type typeFromHandle = typeof(ReturnValueContainer);
			Type[] genericArguments = type.GetGenericArguments();
			int i = 0;
			for (int num = genericArguments.Length; i < num; i++)
			{
				ReturnValueContainer obj = PushIndex();
				object value2 = type.GetProperty("Item" + (i + 1)).GetValue(value, null);
				typeFromHandle.GetMethod("Set", new Type[1] { genericArguments[i] }).Invoke(obj, new object[1] { value2 });
			}
		}

		public void Set(Building building)
		{
			Set(building.ID);
		}

		public void Set(ActionableObject obj)
		{
			Set(obj.ID);
		}

		public void Set(TransactionUpdateData updateData)
		{
			ReturnValueContainer returnValueContainer = SetKey("Type");
			returnValueContainer.Set((int)updateData.Type);
			ReturnValueContainer returnValueContainer2 = SetKey("TransactionId");
			returnValueContainer2.Set(updateData.TransactionId);
			ReturnValueContainer returnValueContainer3 = SetKey("InstanceId");
			returnValueContainer3.Set(updateData.InstanceId);
		}

		public void Set(TransactionDefinition definition)
		{
			Set(definition.ID);
		}

		public void Set(OrderBoardTicket ticket)
		{
			Set(ticket.CharacterDefinitionId);
		}

		public void Set(ZoomType args)
		{
			Set((int)args);
		}

		public void Set(Vector3 value)
		{
			SetKey("x").Set(value.x);
			SetKey("y").Set(value.y);
			SetKey("z").Set(value.z);
		}

		public void Set(int value)
		{
			Set((float)value);
		}

		public void Set(long value)
		{
			Set(Convert.ToInt32(value));
		}

		public void Set(float value)
		{
			type = ValueType.Number;
			numberValue = value;
		}

		public void Set(double value)
		{
			Set((float)value);
		}

		public void Set(string value)
		{
			type = ValueType.String;
			stringValue = value;
		}

		public void Set(bool value)
		{
			type = ValueType.Boolean;
			boolValue = value;
		}

		public void SetNil()
		{
			type = ValueType.Nil;
		}

		public void SetVoid()
		{
			type = ValueType.Void;
		}

		public ReturnValueContainer SetKey(string key)
		{
			type = ValueType.Dictionary;
			return GetContainerForKey(key);
		}

		public ReturnValueContainer PushIndex()
		{
			type = ValueType.Array;
			return GetContainerForNextIndex();
		}

		public void SetEmptyArray()
		{
			type = ValueType.Array;
			ClearArray();
		}
	}
}
