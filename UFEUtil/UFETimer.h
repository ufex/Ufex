// UFETimer.h



namespace UniversalFileExplorer
{
	public ref class UFETimer
	{
	public:
		UFETimer();

		void Start();
		void Stop();
		unsigned int GetTime();

	private:
		unsigned int m_Start;
		unsigned int m_Stop;
	};

	public ref class WinMMTimer
	{
	public:
		WinMMTimer();

		void Start();
		void Stop();
		DWORD GetTime();
		DWORD GetCurTime();


	private:
		DWORD m_Start;
		DWORD m_Stop;
	};

};