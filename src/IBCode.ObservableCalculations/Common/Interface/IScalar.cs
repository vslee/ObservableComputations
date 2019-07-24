﻿using System;

namespace IBCode.ObservableCalculations.Common.Interface
{
	public interface IScalar<ValueType> : IScalar
	{
		ValueType Value { get; set;}
	}

	public interface IScalar : System.ComponentModel.INotifyPropertyChanged
	{
		object ValueObject { get; set;}
		Type Type {get;}
	}

	public interface IReadScalar<out ValueType> : System.ComponentModel.INotifyPropertyChanged
	{
		ValueType Value { get;}
	}

	public interface IWriteScalar<in ValueType> : System.ComponentModel.INotifyPropertyChanged
	{
		ValueType Value { set;}
	}
}
