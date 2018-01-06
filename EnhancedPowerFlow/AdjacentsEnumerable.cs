using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeniusIsme
{
class AdjacentsEnumerable<T> : IEnumerable<T>
{
	AdjacentsSurveyor surveyor;
	List<T> adjustents;

	AdjacentsEnumerable(AdjacentsSurveyor surveyor)
	{
		this.surveyor = surveyor;
		adjustents = new List<T>();
	}

	void LowFrequencyUpdate()
	{

	}
}
}
