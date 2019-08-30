﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBCode.ObservableCalculations.Common.Interface
{
	public interface IConsistent
	{
		bool Consistent {get;}
		event EventHandler ConsistencyRestored;

	}
}
