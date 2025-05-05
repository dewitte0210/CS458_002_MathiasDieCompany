import React, { useState, useEffect } from 'react'
import Fraction from 'fraction.js'

export default function FeatureConfig() {
    const [featureData, setFeatureData] = useState([])
    
    useEffect(() => {
        async function loadData() {
            const apiData = await fetch(`${process.env.REACT_APP_API_BASEURL}api/Pricing/GetFeaturePrices`);
            const data = await apiData.json();
            setFeatureData(data)
        }
        loadData()
    }, [])
    
    const tableStyle = {
        marginTop: '1rem',
        marginLeft: '1rem',
        borderCollapse: 'collapse',
        fontSize: '0.85rem'
    }

    const dataStyle = {
        padding: '3px'
    }
    
    const featureEnum = {
        0: "Error",
        8: "Group 1A1",
        9: "Group 1A2",
        10: "Group 1B1",
        11: "Group 1B2",
        12: "Group 3",
        13: "Group 4",
        14: "Group 5",
        15: "Group 1C",
        16: "Group 6",
        17: "Group 2A1",
        18: "Group 2A2",
        19: "Group 7",
        20: "Group 8",
        21: "Group 9",
        22: "Group 10",
        23: "Group 11",
        24: "Group 12A",
        25: "Group 12B",
        26: "Group 13",
        27: "Group 14",
        28: "Group 17",
        29: "Group S1",
        30: "Group S2",
        31: "Group S3",
        32: "Group S4"
    }
    
    const handleSubmit = async (e) => {
        e.preventDefault()
        try{
            const newFeaturePrices = featureData.map(({setupRate, runRate, difficultyFactor, quantity, maxRadius, type}) => ({
                setupRate: new Fraction(setupRate).valueOf(),
                runRate: new Fraction(runRate).valueOf(),
                difficultyFactor: new Fraction(difficultyFactor).valueOf(),
                quantity: new Fraction(quantity).valueOf(),
                maxRadius: new Fraction(maxRadius).valueOf(),
                type: type,
            }))
            const postData = {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(newFeaturePrices)
            }
            await fetch(`${process.env.REACT_APP_API_BASEURL}api/Pricing/UpdateFeaturePrice`, postData)
            alert("Successfully Updated Feature prices")
        }catch(err){
            alert("Error in updating features. Check that all values are numbers")
        }
    }
    
    const handleChange = (index, field, value) => {
        const updatedRows = [...featureData]
        updatedRows[index][field] = value
        setFeatureData(updatedRows)
    }
    const formStyle = {
        display: 'flex',
        alignItems: "flex-start",
        gap: "1rem"
    }
    return (
        <>
            <form style={formStyle} method="post" onSubmit={handleSubmit}>
                <table style={tableStyle}>
                    <thead>
                        <tr>
                            <th style={dataStyle}>Group Name</th>
                            <th style={dataStyle}>Setup Rate</th>
                            <th style={dataStyle}>Run Rate</th>
                            <th style={dataStyle}>Difficulty Factor</th>
                            <th style={dataStyle}>Quantity</th>
                            <th style={dataStyle}>Max Radius</th>
                        </tr>
                    </thead>
                    <tbody>
                    {featureData?.map((item, i) => (
                        <tr key={i}>
                            <td style={dataStyle}>{featureEnum[item?.type ?? 0]}</td>
                            <td style={dataStyle}><input value={item?.setupRate} onChange={(e) => handleChange(i, "setupRate", e.target.value)} /></td>
                            <td style={dataStyle}><input value={item?.runRate} onChange={(e) => handleChange(i, "runRate", e.target.value)} /></td>
                            <td style={dataStyle}><input value={item?.difficultyFactor} onChange={(e) => handleChange(i, "difficultyFactor", e.target.value)} /></td>
                            <td style={dataStyle}><input value={item?.quantity} onChange={(e) => handleChange(i, "quantity", e.target.value)} /></td>
                            <td style={dataStyle}><input value={item?.maxRadius} onChange={(e) => handleChange(i, "maxRadius", e.target.value)} /></td>
                        </tr>
                    ))}
                    </tbody>
                </table>
                <button style={{marginTop: "15px"}} type='submit'>Apply Changes</button>
            </form>
        </>
    )
}