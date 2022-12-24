﻿using ACadSharp.Entities;

namespace ACadSharp.IO.DWG
{
	internal partial class DwgObjectWriter : DwgSectionIO
	{
		private void writeEntity(Entity entity)
		{
			this.writeCommonEntityData(entity);

			switch (entity)
			{
				case Arc arc:
					this.writeArc(arc);
					break;
				case Circle circle:
					this.writeCircle(circle);
					break;
				case Ellipse ellipse:
					this.writeEllipse(ellipse);
					break;
				case Line l:
					this.writeLine(l);
					break;
				case Point p:
					this.writePoint(p);
					break;
				default:
					this.notify($"Entity not implemented : {entity.GetType().FullName}", NotificationType.NotImplemented);
					return;
			}

			this.registerObject(entity);
		}

		private void writeArc(Arc arc)
		{
			//this.writeCircle(arc);
			this._writer.Write3BitDouble(arc.Center);
			this._writer.WriteBitDouble(arc.Radius);
			this._writer.WriteBitThickness(arc.Thickness);
			this._writer.WriteBitExtrusion(arc.Normal);

			this._writer.WriteBitDouble(arc.StartAngle);
			this._writer.WriteBitDouble(arc.EndAngle);
		}

		private void writeCircle(Circle circle)
		{
			this._writer.Write3BitDouble(circle.Center);
			this._writer.WriteBitDouble(circle.Radius);
			this._writer.WriteBitThickness(circle.Thickness);
			this._writer.WriteBitExtrusion(circle.Normal);
		}

		private void writeEllipse(Ellipse ellipse)
		{
			this._writer.Write3BitDouble(ellipse.Center);
			this._writer.Write3BitDouble(ellipse.EndPoint);
			this._writer.Write3BitDouble(ellipse.Normal);
			this._writer.WriteBitDouble(ellipse.RadiusRatio);
			this._writer.WriteBitDouble(ellipse.StartParameter);
			this._writer.WriteBitDouble(ellipse.EndParameter);
		}

		private void writeLine(Line line)
		{
			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//Start pt 3BD 10
				this._writer.Write3BitDouble(line.StartPoint);
				//End pt 3BD 11
				this._writer.Write3BitDouble(line.EndPoint);
			}


			//R2000+:
			if (this.R2000Plus)
			{
				//Z’s are zero bit B
				bool flag = line.StartPoint.Z == 0.0 && line.EndPoint.Z == 0.0;
				this._writer.WriteBit(flag);

				//Start Point x RD 10
				this._writer.WriteRawDouble(line.StartPoint.X);
				//End Point x DD 11 Use 10 value for default
				this._writer.WriteBitDoubleWithDefault(line.EndPoint.X, line.StartPoint.X);
				//Start Point y RD 20
				this._writer.WriteRawDouble(line.StartPoint.Y);
				//End Point y DD 21 Use 20 value for default
				this._writer.WriteBitDoubleWithDefault(line.EndPoint.Y, line.StartPoint.Y);

				if (!flag)
				{
					//Start Point z RD 30 Present only if “Z’s are zero bit” is 0
					this._writer.WriteRawDouble(line.StartPoint.Z);
					//End Point z DD 31 Present only if “Z’s are zero bit” is 0, use 30 value for default.
					this._writer.WriteBitDoubleWithDefault(line.EndPoint.Z, line.StartPoint.Z);
				}
			}

			//Common:
			//Thickness BT 39
			this._writer.WriteBitThickness(line.Thickness);
			//Extrusion BE 210
			this._writer.WriteBitExtrusion(line.Normal);
		}

		private void writePoint(Point point)
		{
			//Point 3BD 10
			this._writer.Write3BitDouble(point.Location);
			//Thickness BT 39
			this._writer.WriteBitThickness(point.Thickness);
			//Extrusion BE 210
			this._writer.WriteBitExtrusion(point.Normal);
			//X - axis ang BD 50 See DXF documentation
			this._writer.WriteBitDouble(point.Rotation);
		}
	}
}
