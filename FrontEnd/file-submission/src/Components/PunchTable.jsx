import React, { useState, useEffect, useRef } from "react";
import Fraction from 'fraction.js'


export default function PunchTable({ tableData, tableID }) {
    const [rows, setRows] = useState([]);
    const hasInitialized = useRef(false);
    
    useEffect(() => {
        if(!hasInitialized.current  && tableData?.length > 0){
            const transformed = tableData.map(({cutSize, baseSize, setupCost, runCost}) => ({
                cutSize: new Fraction(cutSize).toFraction(true).toString(),
                baseSize: new Fraction(baseSize).toFraction(true).toString(),
                setupCost,
                runCost
            }))
            setRows(transformed)
            hasInitialized.current = true;
        }
    }, [tableData])
    const tableStyle = {
        borderCollapse: 'collapse',
        fontSize: '0.85rem'
    }
    const dataStyle = {
        padding: '3px'
    }
    const divStyle = {
        position: 'absolute',
        top: '75px',
        bottom: 0,
        overflowY: 'auto',
        display: 'flex'
    }
    
    const buttonStyle = {
        flexGrow: '1',
        marginLeft: '700px'
    }
    
    async function handleSubmit(e) {
        e.preventDefault()
        if (true) { // This will be for the confirmation but couldn't get it working.
            try{
                const newPunchData = rows.map(({cutSize, baseSize, setupCost, runCost}) => ({
                    cutSize: new Fraction(cutSize).valueOf(),
                    baseSize: new Fraction(baseSize).valueOf(),
                    setupCost: new Fraction(setupCost).valueOf(),
                    runCost: new Fraction(runCost).valueOf()
                }))
                const postData = {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(newPunchData)
                }
                await fetch(`${process.env.REACT_APP_API_BASEURL}api/Pricing/UpdatePunchPrice/${tableID}`, postData)
                alert("Successfully Updated Table")
            }catch(e){
                console.error(e)
                alert("Error in updating table. Check that all values are numbers")
            }
        }
        
    }
    
    const handleInputChange = (index, field, value) => {
        const updatedRows = [...rows]
        updatedRows[index][field] = value
        setRows(updatedRows)
    }
    
  
    
    return (
        <>
            <form method="post" onSubmit={handleSubmit}>
                <div style={divStyle}>
                    <table style={tableStyle}>
                        <thead>
                        <tr>
                            <th style={dataStyle}>Cut Size</th>
                            <th style={dataStyle}>Base Size</th>
                            <th style={dataStyle}>Setup Cost</th>
                            <th style={dataStyle}>Run Cost</th>
                        </tr>
                        </thead>
                        <tbody style={dataStyle}>
                        {rows?.map((row, index) => (
                            <tr key={index}>
                                <td style={dataStyle}><input name={`c${index}`} value={row?.cutSize}  onChange={(e) => handleInputChange(index, 'cutSize', e.target.value)}/></td>
                                <td style={dataStyle}><input name={`b${index}`} value={row?.baseSize} onChange={(e) => handleInputChange(index, 'baseSize', e.target.value)}/></td>
                                <td style={dataStyle}><input name={`s${index}`} value={row?.setupCost} onChange={(e) => handleInputChange(index, 'setupCost', e.target.value)}/></td>
                                <td style={dataStyle}><input name={`r${index}`} value={row?.runCost} onChange={(e) => handleInputChange(index, 'runCost', e.target.value)}/></td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </div>
                <div style={buttonStyle}>
                    <button type='submit'>Apply Changes</button>
                </div>
            </form>
        </>
        
    )
}