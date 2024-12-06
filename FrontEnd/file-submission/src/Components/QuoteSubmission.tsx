import * as React from "react";
import { useState } from "react";
import { Button } from "react-bootstrap";
import { MdQuestionMark } from "react-icons/md";

interface QuoteSubmissionProps {
  jsonResponse: any[];
  backToUpload: () => void;
}

const QuoteSubmission: React.FC<QuoteSubmissionProps> = ({
  jsonResponse,
  backToUpload,
}) => {
  // Initialize state with the JSON response
  const [data, setData] = useState(jsonResponse);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [formFields, setFormFields] = useState({
    ruleType: "",
    ejecMethod: "",
  });
  const [priceJSON, setPriceJSON] = useState(null);

  // Handle input change
  const handleChange = (
    key: string,
    value: any,
    groupIndex?: number,
    featureIndex?: number
  ) => {
    setData((prev) =>
      prev.map((group, gIdx) => {
        if (gIdx === groupIndex) {
          return {
            ...group,
            features: group.features.map((feature, fIdx) => {
              if (fIdx === featureIndex) {
                return {
                  ...feature,
                  [key]: value, // Update the specific field
                };
              }
              return feature;
            }),
          };
        }
        return group;
      })
    );
  };
  const backToForm = () => {
    setIsSubmitted(false);
  }

  const handleAddFeature = (groupIndex: number) => {
    setData((prev) =>
      prev.map((group, idx) => {
        if (idx === groupIndex) {
          return {
            ...group,
            features: [
              ...group.features,
              {
                newFeature: true,
                count: 1,
                FeatureType: "",
                perimeter: 0,
                diameter: 0,
                multipleRadius: false,
                kissCut: false,
                EntityList: [],
              },
            ],
          };
        }
        return group;
      })
    );
  };

  const handleDeleteFeature = (groupIndex: number, featureIndex: number) => {
    setData((prev) =>
      prev.map((group, idx) => {
        if (idx === groupIndex) {
          return {
            ...group,
            features: group.features.map((feature, fIdx) => {
              if (fIdx === featureIndex) {
                if (feature.count > 1) {
                  return { ...feature, count: feature.count - 1 };
                }
                return null; // Mark for removal
              }
              return feature;
            }).filter((feature) => feature !== null), // Remove null entries
          };
        }
        return group;
      })
    );
  };

  // Handle form submission
  const handleSubmit = async (event: React.FormEvent) => {
    setIsLoading(true); // Start loading
    event.preventDefault();

    const updatedData = data.map((item) => {
      if (item.FeatureType === "Punch") {
        return {
          ...item,
          FeatureType: item.punchType, // Set FeatureType to punchType value
        };
      }
      return item;
    });

    const formData = new FormData();
    const form = event.currentTarget as HTMLFormElement;
    formData.append("ruleType", form.ruleType.value);
    formData.append("ejecMethod", form.ejecMethod.value);
    formData.append(
      "features",
      new Blob([JSON.stringify(updatedData)], { type: "application/json" })
    );

    var object: any = {};
    formData.forEach((value, key) => {
      if (value instanceof Blob && value.type === "application/json") {
        const reader = new FileReader();
        reader.onload = () => {
          object[key] = JSON.parse(reader.result as string);
        };
        reader.readAsText(value);
      } else {
        object[key] = value;
      }
    });

    // Wait for all FileReader operations to complete
    await new Promise((resolve) => setTimeout(resolve, 100));

    var formJSON = JSON.stringify(object);

    //display the form data
    console.log(formJSON);

    try {
      const res = await fetch(
        "https://localhost:44373/api/Pricing/estimatePrice",
        {
          method: "POST",
          body: formJSON,
          headers: new Headers({ "content-type": "application/json" }),
        }
      );

      if (!res.ok)
        throw new Error(`Server error: ${res.status} ${res.statusText}`);
      setPriceJSON(await res.json()); // Store response in state
    } catch (error) {
      alert("An error occurred while submitting your quote. Please try again.");
    } finally {
      setIsSubmitted(true);
      setIsLoading(false); // End loading
    }
  };

  return (
    <div className="quote-container">
      {isLoading ? (
        <div className="loader"></div>
      ) : isSubmitted ? (
        <div className="submission-message">
          <p>Your estimated price is: {priceJSON} </p>
          <div className="button-container">
            <button className="animated-button" onClick={backToForm}>
              <span>Back to Feature List</span>
              <span></span>
            </button>
            <button className="animated-button" onClick={backToUpload}>
              <span>Back to File Upload</span>
              <span></span>
            </button>
          </div>
        </div>
      ) : (
        <>
          <form id="quote-form" onSubmit={handleSubmit} className="quote-form">
            <div className="quote-form-fields">
              <div className="quote-form-label-and-select">
                <div className="quote-form-label">
                  <label htmlFor="ruleType">Rule Type</label>
                  <MdQuestionMark className="question-icon" />
                </div>
                <select
                  id="ruleType"
                  name="ruleType"
                  required
                  value={formFields.ruleType}
                  onChange={(e) => handleChange("ruleType", e.target.value)}
                >
                  <option disabled selected value="">
                    Select Rule Type
                  </option>
                  <option value="2ptCB937">
                    2pt CB Center Bevel .937/.918
                  </option>
                  <option value="2ptSB937">
                    2pt SB Single (Side) Bevel .937/.918
                  </option>
                  <option value="2ptDDB937">
                    2pt DDB Double Double (Facet) Bevel .937/.918
                  </option>
                  <option value="2ptCB1125">
                    2pt CB Center Bevel 1.125/1.250
                  </option>
                  <option value="3ptCB937">
                    3pt CB Center Bevel .937/.918
                  </option>
                  <option value="3ptSB937">
                    3pt SB Single (Side) Bevel .937/.918
                  </option>
                  <option value="3ptDDB937">
                    3pt DDB Double Double (Facet) Bevel .937/.918
                  </option>
                  <option value="3ptDSB937">
                    3pt DSB Double Side Bevel .937/.918
                  </option>
                  <option value="412CB472">
                    .4mm x 12mm CB Center Bevel (.472)
                  </option>
                  <option value="512CB472">
                    .5mm x 12mm CB Center Bevel (.472)
                  </option>
                </select>
              </div>

              <div className="quote-form-label-and-select">
                <div className="quote-form-label">
                  <label htmlFor="ejecMethod">Ejection Method</label>
                  <MdQuestionMark className="question-icon" />
                </div>
                <select
                  id="ejecMethod"
                  name="ejecMethod"
                  required
                  value={formFields.ejecMethod}
                  onChange={(e) => handleChange("ejecMethod", e.target.value)}
                >
                  <option disabled selected value="">
                    Select Ejection Method
                  </option>
                  <option value="StandardSolidSheet">
                    Standard Solid Sheet
                  </option>
                  <option value="StandardHandPlug">Standard Hand Plug</option>
                  <option value="EjectorPlates">Ejector Plates</option>
                </select>
              </div>
            </div>
            <div className="features-table">
              <table>
                <thead>
                  <tr>
                    <th>Count</th>
                    <th>Group</th>
                    <th>Perimeter/Diameter</th>
                    <th>Multiple Radius</th>
                    <th>Kiss Cut</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {data.map((group, groupIndex) => (
                    <>
                      <tr key={`group-${groupIndex}`}>
                        <td colSpan={6} className="identical-text">Number of identical dies: {group.Count}</td>
                      </tr>
                      {group.features.map((feature, featureIndex) => (
                        <tr key={`${groupIndex}-${featureIndex}`}>
                          {feature.newFeature ? (
                            <>
                              <td>
                                <input
                                  className="count-input"
                                  type="number"
                                  value={feature.count}
                                  onChange={(e) => handleChange(
                                    "count",
                                    parseInt(e.target.value),
                                    groupIndex,
                                    featureIndex
                                  )}
                                  required />
                              </td>
                              <td>
                                <select
                                  value={feature.FeatureType}
                                  onChange={(e) => handleChange(
                                    "FeatureType",
                                    e.target.value,
                                    groupIndex,
                                    featureIndex
                                  )}
                                  required
                                >
                                  <option disabled selected value="">
                                    Select Feature Type
                                  </option>
                                  <option value="Group1A1">Group1A1</option>
                                  <option value="Group1A2">Group1A2</option>
                                  <option value="Group1B1">Group1B1</option>
                                  <option value="Group1B2">Group1B2</option>
                                  <option value="Group1C">Group1C</option>
                                  <option value="Group2A">Group2A</option>
                                  <option value="Group3">Group3</option>
                                  <option value="Group4">Group4</option>
                                  <option value="Group5">Group5</option>
                                  <option value="Group6">Group6</option>
                                  <option value="HDSideOutlet">
                                    HD Side Outlet
                                  </option>
                                  <option value="Punch">Punch</option>
                                  <option value="SideOutlet">Side Outlet</option>
                                  <option value="SideTubePunch">
                                    Side Tube Punch
                                  </option>
                                  <option value="StdFTPunch">Std FT Punch</option>
                                  <option value="StdSWPunch">Std SW Punch</option>
                                  <option value="StdRetractPins">
                                    Std Retract Pins
                                  </option>
                                </select>
                              </td>
                              <td>
                                <input
                                  className="perimeter-input"
                                  type="number"
                                  value={feature.perimeter}
                                  onChange={(e) => handleChange(
                                    "perimeter",
                                    parseFloat(e.target.value),
                                    groupIndex,
                                    featureIndex
                                  )}
                                  required />
                              </td>
                              <td>
                                <input
                                  type="checkbox"
                                  checked={feature.multipleRadius}
                                  onChange={(e) => handleChange(
                                    "multipleRadius",
                                    e.target.checked,
                                    groupIndex,
                                    featureIndex
                                  )} />
                              </td>
                              <td>
                                <input
                                  type="checkbox"
                                  checked={feature.kissCut}
                                  onChange={(e) => handleChange(
                                    "kissCut",
                                    e.target.checked,
                                    groupIndex,
                                    featureIndex
                                  )} />
                              </td>
                              <td>
                                <Button
                                  type="button"
                                  variant="danger"
                                  onClick={() => handleDeleteFeature(groupIndex, featureIndex)}
                                >
                                  Delete
                                </Button>
                              </td>
                            </>
                          ) : (
                            <>
                              <td>{feature.count}</td>
                              <td>
                                {feature.FeatureType === "Punch" ? (
                                  <select
                                    value={feature.punchType || ""}
                                    onChange={(e) => handleChange(
                                      "punchType",
                                      e.target.value,
                                      groupIndex,
                                      featureIndex
                                    )}
                                    required
                                  >
                                    <option disabled selected value="">
                                      Select Punch Type
                                    </option>
                                    <option value="SideTubePunch">
                                      Side Tube Punch
                                    </option>
                                    <option value="SideOutlet">
                                      Side Outlet
                                    </option>
                                    <option value="HDSideOutlet">
                                      HD Side Outlet
                                    </option>
                                    <option value="StdFTPunch">
                                      Std FT Punch
                                    </option>
                                    <option value="StdSWPunch">
                                      Std SW Punch
                                    </option>
                                    <option value="StdRetractPins">
                                      Std Retract Pins
                                    </option>
                                  </select>
                                ) : (
                                  feature.FeatureType
                                )}
                              </td>
                              <td>{feature.diameter !== 0 ? feature.diameter.toFixed(3) : feature.perimeter.toFixed(3)}</td>
                              <td>
                                {feature.multipleRadius ? (
                                  <span className="checkmark">&#10003;</span>
                                ) : (
                                  <span className="crossmark">&#10005;</span>
                                )}
                              </td>
                              <td>
                                {feature.kissCut ? (
                                  <span className="checkmark">&#10003;</span>
                                ) : (
                                  <span className="crossmark">&#10005;</span>
                                )}
                              </td>
                              <td>
                                <Button
                                  type="button"
                                  variant="danger"
                                  onClick={() => handleDeleteFeature(groupIndex, featureIndex)}
                                >
                                  Delete
                                </Button>
                              </td>
                            </>
                          )}
                        </tr>
                      ))}
                    </>
                  ))}
                </tbody>
              </table>
              <div className="add-feature">
                <button
                  className="animated-button"
                  type="button"
                  onClick={() => handleAddFeature(0)}
                >
                  <span>Add Feature</span>
                  <span></span>
                </button>
              </div>
            </div>
          </form>
          <div className="button-container">
            <button form="quote-form" type="submit" className="animated-button">
              <span>Confirm</span>
              <span></span>
            </button>
            <button className="animated-button" onClick={backToUpload}>
              <span>Go Back</span>
              <span></span>
            </button>
          </div>
        </>
      )}
    </div>
  );
};

export default QuoteSubmission;
