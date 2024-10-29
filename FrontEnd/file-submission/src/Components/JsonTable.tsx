import * as React from 'react';

/*
  Renders JSON data as a table.
*/
interface JsonTableProps {
  jsonResponse: any[];
}

const JsonTable: React.FC<JsonTableProps> = ({ jsonResponse }) => {
  // Calculate the total number of features detected
  const numFeatures = jsonResponse?.reduce((acc: number, info: any) => acc + info.count, 0);

  return (
    // Display JSON data in a table
    <div className="json-response">
      <h3>Features Detected: {numFeatures}</h3>
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
          {jsonResponse.map((info) => {
            // Check if the feature is a circle and calculate the diameter if it is
            const perimeterOrDiameter = (info.featureType === "Group1B" && info.entityList.length === 1)
              ? info.perimeter / Math.PI
              : info.perimeter;
            return (
              <tr key={info.featureType}>
                <td>{info.count}</td>
                <td>{info.featureType}</td>
                <td>{perimeterOrDiameter.toFixed(3)}</td>
                <td>{info.multipleRadius ? <span className="checkmark">&#10003;</span> : <span className="crossmark">&#10005;</span>}</td>
                <td>{info.kissCut ? <span className="checkmark">&#10003;</span> : <span className="crossmark">&#10005;</span>}</td>
                <td>{info.border ? <span className="checkmark">&#10003;</span> : <span className="crossmark">&#10005;</span>}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};

export default JsonTable;
