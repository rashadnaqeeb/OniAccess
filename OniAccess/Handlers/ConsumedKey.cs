namespace OniAccess.Handlers {
	public struct ConsumedKey {
		public KKeyCode KeyCode;
		public Modifier Modifier;

		public ConsumedKey(KKeyCode keyCode, Modifier modifier = Modifier.None) {
			KeyCode = keyCode;
			Modifier = modifier;
		}
	}
}
