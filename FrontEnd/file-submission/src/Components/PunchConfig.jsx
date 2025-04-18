import React, {useEffect, useState} from 'react'
import PunchTable from "./PunchTable.jsx";


export default function PunchConfig() {
    const [activePunchSection, setActivePunchSection] = useState("SO");
    const [punchData, setPunchData] = useState({});
    useEffect(() => {
        async function loadPunchData() {
            const apiData = await fetch(`${process.env.REACT_APP_API_BASEURL}api/Pricing/GetPunchPrices`)
            const data = await apiData.json();
            setPunchData(data)
        }
        loadPunchData();
    }, [])
    const punchTypes = {
        textDecoration: 'none',
        color: '#e0e0e0'
    }
    const punchDiv = {
        borderBottom: '1px solid black'
    }
    const punchBar = {
        backgroundColor: '#606060',
        width: 'fit-content',
        padding: '5px',
        height: '94vh'
    }
    
    const contentDiv = {
        padding: '5px'
    }
    
    const handleSO = (e) => {
        e.preventDefault()
        setActivePunchSection("SO");
    }
    const handleFT = (e) => {
        e.preventDefault()
        setActivePunchSection("FT");
    }
    const handleSW = (e) => {
        e.preventDefault()
        setActivePunchSection("SW");
    }
    const handleTube = (e) => {
        e.preventDefault()
        setActivePunchSection("Tube");
    }
    const handleHDSO = (e) => {
        e.preventDefault()
        setActivePunchSection("HDSO");
    }
    const handleSTD = (e) => {
        e.preventDefault()
        setActivePunchSection("STD");
    }
    
    const handleEnter = (e) => {
        e.currentTarget.style.backgroundColor = '#909090'
    }
    const handleLeave = (e) => {
        e.currentTarget.style.backgroundColor = '#606060'
    }
    return (
        <div style={{display: 'flex'}}>
            <div style={punchBar}>
                <a onClick={handleSO} style={punchTypes} href='punches/SOpunch'>
                    <div style={punchDiv} onMouseEnter={handleEnter} onMouseLeave={handleLeave}>
                        Side Outlet Punches
                    </div>
                </a>
                <a onClick={handleFT} style={punchTypes} href='punches/FTpunch'>
                    <div style={punchDiv} onMouseEnter={handleEnter} onMouseLeave={handleLeave}>
                        Feed Through Punches
                    </div>
                </a>
                <a onClick={handleSW} style={punchTypes} href='punches/SWpunch'>
                    <div style={punchDiv} onMouseEnter={handleEnter} onMouseLeave={handleLeave}>
                        Straight Wall Punches
                    </div>
                </a>
                <a onClick={handleTube} style={punchTypes} href='punches/TubePunch'>
                    <div style={punchDiv} onMouseEnter={handleEnter} onMouseLeave={handleLeave}>
                        Tube Punches
                    </div>
                </a>
                <a onClick={handleHDSO} style={punchTypes} href='punches/HDSOpunch'>
                    <div style={punchDiv} onMouseEnter={handleEnter} onMouseLeave={handleLeave}>
                        Heavy Duty Side Outlet Punches
                    </div>
                </a>
                <a onClick={handleSTD} style={punchTypes} href='punches/STDpins'>
                    <div style={punchDiv} onMouseEnter={handleEnter} onMouseLeave={handleLeave}>
                        Standard retractable Pins
                    </div>
                </a>
            </div>
            
            {activePunchSection === "SO" && (
                <div style={contentDiv}>
                    Side Outlet Punches
                    <PunchTable tableData={punchData?.soPunchList} tableID={2}/>
                </div>
            )}
            {activePunchSection === "FT" && (
                <div style={contentDiv}>
                    Feed Through Punches
                    <PunchTable tableData={punchData?.ftPunchList} tableID={4}/>
                </div>
            )}
            {activePunchSection === "SW" && (
                <div style={contentDiv}>
                    Side Wall Punches
                    <PunchTable tableData={punchData?.swPunchList} tableID={5}/>
                </div>
            )}
            {activePunchSection === "Tube" && (
                <div style={contentDiv}>
                    Tube Punches
                    <PunchTable tableData={punchData?.tubePunchList} tableID={1}/>
                </div>
            )}
            {activePunchSection === "HDSO" && (
                <div style={contentDiv}>
                    Heavy Duty Side Outlet Punches
                    <PunchTable tableData={punchData?.hdsoPunchList} tableID={3}/>
                </div>
            )}
            {activePunchSection === "STD" && (
                <div style={contentDiv}>
                    Standard retractable Pins
                    <PunchTable tableData={punchData?.retractList} tableID={6}/>
                </div>
            )}
            
        </div>
        
    )
}