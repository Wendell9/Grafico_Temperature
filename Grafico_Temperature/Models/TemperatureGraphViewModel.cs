namespace Grafico_Temperature.Models
{
	public class TemperatureGraphViewModel
	{
		public List<double> Temperatures { get; set; }
		public List<string> Timestamps { get; set; }
		public double AverageTemperature { get; set; }

		public TemperatureGraphViewModel()
		{
			Temperatures = new List<double>();
			Timestamps = new List<string>();
		}
	}
}
