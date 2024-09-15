using System;

namespace CustomEffects
{
    public class EffectInformation
    {
		private string _name;
		private float _fluidsPerHour;
		private float _caloriesPerHour;
		private float _healthPerHour;
		private float _duration;

		public string Name { get { return _name; } set { _name = value; } }
		public float FluidsPerHour { get { return _fluidsPerHour; } set { _fluidsPerHour = value; } }
		public float CaloriesPerHour { get { return _caloriesPerHour; } set { _caloriesPerHour = value; } }
		public float HealthPerHour { get { return _healthPerHour; } set { _healthPerHour = value; } }
		public float Duration { get { return _duration; } set { _duration = value; } }

        public EffectInformation(string name, float fluidsPerHour = 0f, float caloriesPerHour = 0f, float healthPerHour = 0f, float duration = -1f)
        {
			_name = name;
			_fluidsPerHour = fluidsPerHour;
			_caloriesPerHour = caloriesPerHour;
			_healthPerHour = healthPerHour;
			_duration = duration;
        }
    }
}
