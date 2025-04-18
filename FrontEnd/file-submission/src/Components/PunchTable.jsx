import React, { useState, useEffect } from "react";
import Fraction from 'fraction.js'


export default function PunchTable({ tableData, tableID }) {
    const [rows, setRows] = useState([]);
    const [dataLoaded, setDataLoaded] = useState(false);
    
    useEffect(() => {
        setRows(tableData)
        if(rows?.length > 0 && !dataLoaded){
            setRows(rows.map(({cutSize, baseSize, setupCost, runCost}) => ({
                cutSize: new Fraction(cutSize).toFraction(true).toString(),
                baseSize: new Fraction(baseSize).toFraction(true).toString(),
                setupCost,
                runCost
            })))
        }
        if(rows?.length > 0){
            setDataLoaded(true);
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
    
    function handleSubmit(e) {
        e.preventDefault()
        try{
            const newPunchData = rows.map(({cutSize, baseSize, setupCost, runCost}) => ({
               cutSize: new Fraction(cutSize).valueOf(),
               baseSize: new Fraction(baseSize).valueOf(),
               setupCost: parseFloat(setupCost),
               runCost: parseFloat(runCost)
           }))
            console.log(newPunchData)
        }catch(e){
            console.error(e)
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