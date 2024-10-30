import * as React from 'react';
import { useState } from 'react';

interface QuoteSubmissionProps {
  jsonResponse: any[];
}

const QuoteSubmission: React.FC<QuoteSubmissionProps> = ({ jsonResponse }) => {
  // Initialize state with the JSON response
  const [data, setData] = useState(jsonResponse);

  // Handle input change
  const handleChange = (index: number, key: string, value: any) => {
    const updatedData = [...data];
    updatedData[index] = { ...updatedData[index], [key]: value };
    setData(updatedData);
  };

  // Handle form submission
  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
  };

  return (
    <div className="json-response">
      <form onSubmit={handleSubmit} className="quote-form">
        <label htmlFor="ruleType">Rule Type:</label>
        <select id="ruleType" name="ruleType">
          <option value="2ptCB937">2pt CB Center Bevel .937/.918</option>
          <option value="2ptSB937">2pt SB Single (Side) Bevel .937/.918</option>
          <option value="2ptDDB937">2pt DDB Double Double (Facet) Bevel .937/.918</option>
          <option value="2ptCB1125">2pt CB Center Bevel 1.125/1.250</option>
          <option value="3ptCB937">3pt CB Center Bevel .937/.918</option>
          <option value="3ptSB937">3pt SB Single (Side) Bevel .937/.918</option>
          <option value="3ptDDB937">3pt DDB Double Double (Facet) Bevel .937/.918</option>
          <option value="3ptDSB937">3pt DSB Double Side Bevel .937/.918</option>
          <option value="412CB472">.4mm x 12mm CB Center Bevel (.472)</option>
          <option value="512CB472">.5mm x 12mm CB Center Bevel (.472)</option>
        </select>

        <label htmlFor="ejecMethod">Ejection Method:</label>
        <select id="ejecMethod" name="ejecMethod">
          <option value="StandardSolidSheet">Standard Solid Sheet</option>
          <option value="StandardHandPlug">Standard Hand Plug</option>
          <option value="EjectorPlates">Ejector Plates</option>
        </select>

        <table>
          <thead>
            <tr>
              <th>Count</th>
              <th>Group</th>
              <th>Perimeter/Diameter</th>
              <th>Multiple Radius</th>
              <th>Kiss Cut</th>
              <th>Border</th>
            </tr>
          </thead>
          <tbody>
            {data.map((info, index) => {
              return (
                <tr key={index}>
                  <td>{info.count}</td>
                  <td>
                    {info.featureType === "Punch" ? (
                      <select
                        value={info.punchType || ""}
                        onChange={(e) => handleChange(index, "punchType", e.target.value)}
                      >
                        <option value="">Select Punch Type</option>
                        <option value="SideTubePunch">Side Tube Punch</option>
                        <option value="SideOutlet">Side Outlet</option>
                        <option value="HDSideOutlet">HD Side Outlet</option>
                        <option value="StdFTPunch">Std FT Punch</option>
                        <option value="StdSWPunch">Std SW Punch</option>
                        <option value="StdRetractPins">Std Retract Pins</option>
                      </select>
                    ) : (
                      info.featureType
                    )}
                  </td>
                  <td>{info.perimeter.toFixed(3)}</td>
                  <td>{info.multipleRadius ? <span className="checkmark">&#10003;</span> : <span className="crossmark">&#10005;</span>}</td>
                  <td>{info.kissCut ? <span className="checkmark">&#10003;</span> : <span className="crossmark">&#10005;</span>}</td>
                  <td>{info.border ? <span className="checkmark">&#10003;</span> : <span className="crossmark">&#10005;</span>}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </form>
    </div>
  );
};

export default QuoteSubmission;
