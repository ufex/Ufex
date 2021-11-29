// UFETimer.cpp

#include "Stdafx.h"
#include "UFETimer.h"

namespace UniversalFileExplorer
{
	UFETimer::UFETimer()
	{
		m_Start = 0;
		m_Stop = 0;
	}

	void UFETimer::Start()
	{
		m_Start = clock();
	}
		
	void UFETimer::Stop()
	{
		m_Stop = clock();
	}

	unsigned int UFETimer::GetTime()
	{
		return m_Stop - m_Start;
	}



	WinMMTimer::WinMMTimer()
	{
		m_Start = 0;
		m_Stop = 0;
		TIMECAPS resolution;
		if(timeGetDevCaps(&resolution, sizeof(TIMECAPS)) == TIMERR_NOERROR)
		{
		}
	}

	void WinMMTimer::Start()
	{
		if (timeBeginPeriod(1) == TIMERR_NOERROR)
		{
			m_Start = timeGetTime();
		}
	}

	void WinMMTimer::Stop()
	{
		m_Stop = timeGetTime();
		timeEndPeriod(1);
	}

	DWORD WinMMTimer::GetTime()
	{
		return m_Stop - m_Start;
	}

	DWORD WinMMTimer::GetCurTime()
	{
		return timeGetTime() - m_Start;
	}
};