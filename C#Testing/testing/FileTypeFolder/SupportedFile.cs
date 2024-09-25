/*
 * Abstract class to be inherrited by every File child class
 * - DWG, DXF, PDF
 */
using System;
using System.IO;
using System.Numerics;

abstract public class SupportedFile
{
	protected string path;
	protected SupportedExtensions fileType;
	protected FeatureListClass featureList;
	protected EntityListClass entityList;
	protected enum SupportedExtensions
	{
		pdf,
		dwg,
		dxf,
	}
	public SupportedFile(string path)
	{
		this.path = path;
	}
	public void setPath(string path)
	{
		this.path = path;
	}
	public string getPath()
	{
		return this.path;
	}
	public string getFileType()
	{
		return fileType.ToString();
	}
	public void writeFeatures()
	{
		File.WriteAllLines("features.txt", featureList.toStringArray());
	}
	public void readFeatures()
	{
		featureList.setFeatureList(File.ReadAllLines("features.txt"));
	}
	public FeatureListClass getFeatureList()
	{
		return featureList;
	}
	
	/* 
	 * Method that should be implemented by each child 
	 * This is where the feature recognition logic will go
	*/
	abstract public bool findFeatures();
	// Method to read the data from a file and fill the entityList with entities
	public abstract void readEntities();
}
