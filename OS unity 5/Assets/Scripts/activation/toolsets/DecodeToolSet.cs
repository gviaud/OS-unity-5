using UnityEngine;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class DecodeToolSet{
		
	public DecodeToolSet()
	{
	}
	
	// METHODE PERSOS
	
	//DECODE CODE
	
	public sbyte[] decodeReturnedCode(String code)
	{
		sbyte[] buffer = decodeSBytesUpperCase(code);
		sbyte[] tmpBuffer = new sbyte[buffer.Length-1];
		for(int i=tmpBuffer.Length-1;i>0;i--)
		{
			tmpBuffer.SetValue(buffer[i+1],i);
		}
		
		sbyte[] shiftedBuffer = shiftBuffer(-buffer[0], tmpBuffer);
		for(int i=0;i<shiftedBuffer.Length;i++)
		{
			buffer.SetValue(shiftedBuffer[i],i+1);
		}
		return buffer;		
	}


//			setInfos(buffer);
//			
//			checkDate();
//			if(!checkSum())
//				throw new IllegalArgumentException();
//		} catch (Exception e) {
//			throw new IllegalArgumentException("Invalid values");
//		}

	//DECODE TOOLSET

	public sbyte[] decodeSBytesUpperCase(String code)
	{
		sbyte[] result = new sbyte[code.Length/2];		
		char[] cars = new char[2];
		
		for (int i = 0; i < result.Length; i++)
			{
				cars[0] = code[i*2];	// caractères 2 par 2
				cars[1] = code[i*2 + 1];
				result[i] = decodeSByteUpperCase(cars);
			}
		
		return result;
	}
	
	public byte[] decodeBytesUpperCase(String code)
	{
		byte[] result = new byte[code.Length/2];		
		char[] cars = new char[2];
		
		for (int i = 0; i < result.Length; i++)
			{
				cars[0] = code[i*2];	// caractères 2 par 2
				cars[1] = code[i*2 + 1];
				result[i] = decodeByteUpperCase(cars);
			}
		
		return result;
	}
	
	private sbyte decodeSByteUpperCase(char[] cars) 
	{
		int val = ((((int)cars[0])-65)%10)*26 + (((int)cars[1])-65); // versions lettres de A à Z
		val -= 128;
		
		return (sbyte)val;
	}
	
	private byte decodeByteUpperCase(char[] cars) 
	{
		int val = ((((int)cars[0])-65)%10)*26 + (((int)cars[1])-65); // versions lettres de A à Z
		val -= 128;
		
		return (byte)val;
	}
	
	public sbyte[] shiftBuffer(int shiftValue,sbyte[] buffer) 
	{
		// cas buffer vide
		if(buffer.Length == 0)
			return new sbyte[]{};
		
		sbyte[] result = new sbyte[buffer.Length];
		// valeurs binaires avec n zéro à gauche (11111111, 011111111, 00111111,
		// etc.} classé par index du tableau 
		int[] ZEROS_LEFT  = 
						{0xFF, 0x7F, 0x3F, 0x1F, 0x0F, 0x07, 0x03, 0x01, 0x00};
		// valeurs binaires avec n zéro à droite, classés par index 
		int[] ZEROS_RIGHT = 
						{0xFF, 0xFE, 0xFC, 0xF8, 0xF0, 0xE0, 0xC0, 0x80, 0x00};

		int shift = shiftValue % (buffer.Length * 8);
		if(shift < 0)
			shift = (buffer.Length*8) + shift;
		
		// calcul de l'octet du buffer à copier
		int offset = buffer.Length - 1 - (shift/8);

		int shiftEnd = shift%8;
		int shiftStart = 8 - shiftEnd;
			
		// pour chaque octet résultant
		for (int i = 0; i < result.Length; i++) {
			// première partie à copier (car les 8 bits sont sur 2 octets)
			sbyte start = buffer[offset];
			start <<= shiftStart; 		 // met à zéro la partie de droite
			start &= (sbyte)ZEROS_RIGHT[shiftStart];
			// deuxième partie (boucle si besoin)
			sbyte end = buffer[(offset+1) % buffer.Length];
			end >>= shiftEnd;
			end &= (sbyte)ZEROS_LEFT[shiftEnd]; // met à zéro la partie de gauche
			// Remarque : le décalage est censé mettre des 0 mais parfois il met
			// des 1 même avec >>>=
			
			result[i] = (sbyte)(start | end);
			offset = (offset+1) % buffer.Length; // octet suivant
		}

		return result;
	 }
	
	public int convertBytesToInt(byte[] tab)
	{
//		code fonctionnant que pour des byte[] de taille 4
//		int converted = 0x0;
//		converted = (tab[0]<<24)|(tab[1]<<16)|(tab[2]<<8)|(tab[3]);
//		return converted;
		int converted = 0x0;
		for(int i=0;i<tab.Length;i++)
		{
			int decal = ((tab.Length-1-i)*8);
			converted |= tab[i]<<decal;
		}
		return converted;
	}
	
	public int convertSBytesToInt(sbyte[] tab)
	{
		int converted = 0x0;
		for(int i=0;i<tab.Length;i++)
		{
			int decal = ((tab.Length-1-i)*8);
			converted |= tab[i]<<decal;
		}
		return converted;
	}
	
//	protected sbyte computeCheckSum() {
//		// retourne le nombre de 1 dans la clé, 16 octets => 128 bits = 2 long
//		int count = 0;
//		
//		for (int i = 0; i < _macAddress.length; i++) {
//			count += Bits.bitCount(_macAddress[i]);
//		}
//		count += EncodeToolSet.count_bits(_idSoft);
//		count += EncodeToolSet.count_bits(_revendeur);
//		count += EncodeToolSet.count_bits((sbyte)_endDate);
//		count += EncodeToolSet.count_bits(_random);
//		count += EncodeToolSet.count_bits(_version);
//		count += EncodeToolSet.count_bits(_nbLibraries);
//		
//		return (byte)count; // retourne -128 si tous les bits sont à 1
//	}
}
