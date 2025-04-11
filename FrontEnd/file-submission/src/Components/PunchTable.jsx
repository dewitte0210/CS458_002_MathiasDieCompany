import React from "react";
import Fraction from 'fraction.js'

export default function PunchTable({ tableData }) {
    const tableStyle = {
        borderCollapse: 'collapse',
        fontSize: '0.85rem',
    }
    const dataStyle = {
        padding: '3px'
    }
    const divStyle = {
        position: 'absolute',
        top: '75px',
        bottom: 0,
        overflowY: 'auto'
    }
    return (
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
                    {tableData?.map((row, index) => (
                        <tr key={index}>
                            <td style={dataStyle}>{new Fraction(row?.cutSize).toFraction()}</td>
                            <td style={dataStyle}>{new Fraction(row?.baseSize).toFraction()}</td>
                            <td style={dataStyle}>{row?.setupCost}</td>
                            <td style={dataStyle}>{row?.runCost}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    )
}