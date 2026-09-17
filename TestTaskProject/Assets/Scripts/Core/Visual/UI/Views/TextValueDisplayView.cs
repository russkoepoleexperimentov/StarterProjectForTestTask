using UnityEngine;
using TMPro;


namespace Core.Visual.UI
{
	[RequireComponent(typeof(TMP_Text))]
    public class TextValueDisplayView : StringDisplayView
    {
        private TMP_Text _text;

		public override string Displayed => _text.text;
		
		private void Awake() 
		{
			_text = GetComponent<TMP_Text>();
		}

		public override void DisplayValue(string value)
		{
			_text.text = value;
		}

    }
}