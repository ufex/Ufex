#pragma once

using namespace System;
using namespace System::Data;
using namespace System::Collections;

namespace UniversalFileExplorer
{
	public ref class ArrayUtil
	{

	public:

		static array<Byte>^ LeReverse(array<Byte>^ input)
		{			
			if(BitConverter::IsLittleEndian)
			{
				for(int i = 0; i < input->Length; i++)
				{
					Array::Reverse(input, 0 + (i * 4), 4);
				}
			}
			return input;
		}

		static bool CompareArrays(const array<Byte>^ a1, const array<Byte>^ a2)
		{
			if(a1 == nullptr && a2 == nullptr)
				return true;

			if(a1 == nullptr || a2 == nullptr)
				return false;

			// Return false if the arrays are not the same size
			if(a1->Length != a2->Length)
				return false;

			// Compare the elements in each array
			for(int i = 0; i < a1->Length; i++)
			{
				if(a1[i] != a2[i])
					return false;
			}
			return true;
		}

		static bool CompareArrays(const array<UInt16>^ a1, const array<UInt16>^ a2)
		{
			if(a1 == nullptr && a2 == nullptr)
				return true;

			if(a1 == nullptr || a2 == nullptr)
				return false;

			// Return false if the arrays are not the same size
			if(a1->Length != a2->Length)
				return false;

			// Compare the elements in each array
			for(int i = 0; i < a1->Length; i++)
			{
				if(a1[i] != a2[i])
					return false;
			}
			return true;
		}

		static bool CompareArrays(const array<UInt32>^ a1, const array<UInt32>^ a2)
		{
			if(a1 == nullptr && a2 == nullptr)
				return true;

			if(a1 == nullptr || a2 == nullptr)
				return false;

			// Return false if the arrays are not the same size
			if(a1->Length != a2->Length)
				return false;

			// Compare the elements in each array
			for(int i = 0; i < a1->Length; i++)
			{
				if(a1[i] != a2[i])
					return false;
			}
			return true;
		}


		static bool CompareArrays(const array<UInt64>^ a1, const array<UInt64>^ a2)
		{
			if(a1 == nullptr && a2 == nullptr)
				return true;

			if(a1 == nullptr || a2 == nullptr)
				return false;

			// Return false if the arrays are not the same size
			if(a1->Length != a2->Length)
				return false;

			// Compare the elements in each array
			for(int i = 0; i < a1->Length; i++)
			{
				if(a1[i] != a2[i])
					return false;
			}
			return true;
		}

		static bool CompareArrays(const array<SByte>^ a1, const array<SByte>^ a2)
		{
			if(a1 == nullptr && a2 == nullptr)
				return true;

			if(a1 == nullptr || a2 == nullptr)
				return false;

			// Return false if the arrays are not the same size
			if(a1->Length != a2->Length)
				return false;

			// Compare the elements in each array
			for(int i = 0; i < a1->Length; i++)
			{
				if(a1[i] != a2[i])
					return false;
			}
			return true;
		}

		static bool CompareArrays(const array<Int16>^ a1, const array<Int16>^ a2)
		{
			if(a1 == nullptr && a2 == nullptr)
				return true;

			if(a1 == nullptr || a2 == nullptr)
				return false;

			// Return false if the arrays are not the same size
			if(a1->Length != a2->Length)
				return false;

			// Compare the elements in each array
			for(int i = 0; i < a1->Length; i++)
			{
				if(a1[i] != a2[i])
					return false;
			}
			return true;
		}

		static bool CompareArrays(const array<Int32>^ a1, const array<Int32>^ a2)
		{
			if(a1 == nullptr && a2 == nullptr)
				return true;

			if(a1 == nullptr || a2 == nullptr)
				return false;

			// Return false if the arrays are not the same size
			if(a1->Length != a2->Length)
				return false;

			// Compare the elements in each array
			for(int i = 0; i < a1->Length; i++)
			{
				if(a1[i] != a2[i])
					return false;
			}
			return true;
		}

		static bool CompareArrays(array<System::Object^>^ a1, array<System::Object^>^ a2)
		{
			if(a1 == nullptr && a2 == nullptr)
				return true;

			if(a1 == nullptr || a2 == nullptr)
				return false;

			// Return false if the arrays are not the same size
			if(a1->Length != a2->Length)
				return false;

			// Compare the elements in each array
			for(int i = 0; i < a1->Length; i++)
			{
				if(!a1[i]->Equals(a2[i]))
					return false;
			}
			return true;
		}


		static UInt32 ArraySum(const array<Byte>^ x)
		{
			UInt32 sum = 0;
			for(int i = 0; i < x->Length; i++)
				sum += x[i];
			return sum;
		}

		static UInt32 ArraySum(const array<UInt16>^ x)
		{
			UInt32 sum = 0;
			for(int i = 0; i < x->Length; i++)
				sum += x[i];
			return sum;
		}

		static UInt32 ArraySum(const array<UInt32>^ x)
		{
			UInt32 sum = 0;
			for(int i = 0; i < x->Length; i++)
				sum += x[i];
			return sum;
		}
	};
}