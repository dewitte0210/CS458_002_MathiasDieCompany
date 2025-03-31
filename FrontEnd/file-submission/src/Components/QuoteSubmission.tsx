import * as React from "react";
import {useState} from "react";
import {Button} from "react-bootstrap";
import {translate} from "../translator";

interface QuoteSubmissionProps {
    featureGroups: any[];
    backToUpload: () => void;
}


const QuoteSubmission: React.FC<QuoteSubmissionProps> = ({
                                                             featureGroups,
                                                             backToUpload,
                                                         }) => {
    // Sort the initial data
    const sortedData = React.useMemo(() => {
        return featureGroups.map((group) => ({
            ...group,
            features: [...group.features].sort((a, b) => a.FeatureType.localeCompare(b.FeatureType)),
        }));
    }, [featureGroups]);
    // Initialize states
    const [data, setData] = useState(sortedData);
    const [isLoading, setIsLoading] = useState(false);
    const [isSubmitted, setIsSubmitted] = useState(false);
    const [formFields, setFormFields] = useState({
        ruleType: "",
        ejecMethod: "",
    });
    const [priceJSON, setPriceJSON] = useState<number | null>(null);

    /*
      Event handler for when the user changes a field in the form.
      Updates the state with the new value.
    */
    const handleChange = (
        key: string,
        value: any,
        groupIndex?: number,
        featureIndex?: number
    ) => {
        if (groupIndex === undefined) {
            setFormFields((prev) => ({...prev, [key]: value}));
            return
        }
        setData((prev) =>
            prev.map((group, gIdx) => {
                if (gIdx === groupIndex) {
                    if (key === "Count") {
                        return {
                            ...group,
                            Count: value,
                        };
                    }
                    return {
                        ...group,
                        features: group.features.map((feature, fIdx) => {
                            if (fIdx === featureIndex) {
                                return {
                                    ...feature,
                                    [key]: value,
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


    /*
      Event handler for when the user clicks the back button after submission.
    */
    const backToForm = () => {
        setIsSubmitted(false);
    }

    /*
      Event handler for when the user adds a new feature.
    */
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
                                multipleRadius: 1,
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

    /*
      Event handler for when the user deletes a feature.
    */
    const handleDeleteFeature = (groupIndex: number, featureIndex: number) => {
        setData((prev) =>
            prev.map((group, idx) => {
                if (idx === groupIndex) {
                    return {
                        ...group,
                        features: group.features.map((feature, fIdx) => {
                            if (fIdx === featureIndex) {
                                if (feature.count > 1) {
                                    return {...feature, count: feature.count - 1};
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

    /*
      Event handler for when the user submits the form.
    */
    const handleSubmit = async (event: React.FormEvent) => {
        setIsLoading(true); // Start loading
        event.preventDefault();

        // Update the feature type for punches
        const updatedData = data.map((group) => ({
            ...group,
            features: group.features.map((feature) =>
                feature.FeatureType === "Punch"
                    ? {...feature, FeatureType: feature.punchType}
                    : feature
            ),
        }));

        // Build the form object
        const formObject: any = {
            ruleType: (event.target as HTMLFormElement).ruleType.value,
            ejecMethod: (event.target as HTMLFormElement).ejecMethod.value,
            featureGroups: updatedData,
        };

        // This is the JSON object that will be sent to the server
        const formJSON = JSON.stringify(formObject);

        // Send the form data to the server
        try {
            const res = await fetch(
                `${process.env.REACT_APP_API_BASEURL}api/Pricing/EstimatePrice`,
                {
                    method: "POST",
                    body: formJSON,
                    headers: new Headers({"content-type": "application/json"}), // Set content type to JSON
                }
            );

            if (!res.ok) {
                throw new Error(`Server error: ${res.status} ${res.statusText}`);
            }

            const responseJSON = await res.json();
            setPriceJSON(parseFloat(responseJSON)); // Store the price in state as a float
        } catch (error) {
            console.error("Error occurred during submission:", error);
            alert("An error occurred while submitting your quote. Please try again.");
        } finally {
            setIsSubmitted(true); // Moves to next page
            setIsLoading(false); // End loading
        }
    };


    return (
        <div className="quote-container">
            {isLoading ? (
                <div className="loader"></div>
            ) : isSubmitted ? (
                <div className="submission-message">
                    <p>Your estimated price is: ${priceJSON?.toFixed(2)} </p>
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
                                    {/* <MdQuestionMark className="question-icon" /> */}
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
                                    {/* <MdQuestionMark className="question-icon" /> */}
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
                                    <th>Multiple Radii</th>
                                    <th>Kiss Cut</th>
                                    <th>Actions</th>
                                </tr>
                                </thead>
                                <tbody>
                                {/* Map the JSON data, nested loops */}
                                {data.map((group, groupIndex) => (
                                    <>
                                        <tr key={`group-${groupIndex}`}>
                                            <td colSpan={6} className="identical-text">
                                                Number of identical dies:
                                                <input
                                                    className="count-input-top"
                                                    type="number"
                                                    value={group.Count}
                                                    onChange={(e) => handleChange("Count", parseInt(e.target.value), groupIndex)}
                                                    required
                                                />
                                            </td>
                                        </tr>
                                        {group.features.map((feature, featureIndex) => (
                                            <tr key={`${groupIndex}-${featureIndex}`}>
                                                {/* Different renders based on if it's a new feature or not */}
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
                                                                required/>
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
                                                                <option value="Group1A1" label={translate("Group1A1")}></option>
                                                                <option value="Group1A2" label={translate("Group1A2")}></option>
                                                                <option value="Group1B1" label={translate("Group1B1")}></option>
                                                                <option value="Group1B2" label={translate("Group1B2")}></option>
                                                                <option value="Group1C" label={translate("Group1C")}></option>
                                                                <option value="Group2A1" label={translate("Group2A1")}></option>
                                                                <option value="Group2A2" label={translate("Group2A2")}></option>
                                                                <option value="Group3" label={translate("Group3")}></option>
                                                                <option value="Group4" label={translate("Group4")}></option>
                                                                <option value="Group5" label={translate("Group5")}></option>
                                                                <option value="Group6" label={translate("Group6")}></option>
                                                                <option value="HDSideOutlet"> HD Side Outlet </option>
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
                                                                required/>
                                                        </td>
                                                        <td>
                                                            <input
                                                                className="perimeter-input"
                                                                type="number"
                                                                value={feature.multipleRadius}
                                                                onChange={(e) => handleChange(
                                                                    "multipleRadius",
                                                                    parseFloat(e.target.value),
                                                                    groupIndex,
                                                                    featureIndex
                                                                )}/>
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
                                                                )}/>
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
                                                            {feature.multipleRadius}
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
                </>
            )}
        </div>
    );
};

export default QuoteSubmission;
