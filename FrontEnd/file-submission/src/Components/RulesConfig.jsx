import React, {useState, useEffect} from 'react'
import Fraction from 'fraction.js'

export default function RulesConfig() {
    const [rules, setRules] = useState({})
    useEffect(() => {
        async function loadData() {
            const apiData = await fetch(`${process.env.REACT_APP_API_BASEURL}api/Pricing/GetBaseRates`);
            const data = await apiData.json();
            setRules(data)
        }
        loadData()
    }, [])
    
    const formStyle = {
        display: 'flex',
        alignItems: "flex-start",
        gap: "1rem",
        marginTop: "1rem",
        marginLeft: "1rem",
    }
    
    const handleSubmit = async (e) => {
        e.preventDefault()
        try{
            const newRules = {
                baseShopRate: new Fraction(rules?.baseShopRate).valueOf(),
                dieCuttingShopRate: new Fraction(rules?.dieCuttingShopRate).valueOf(),
                plugRate: new Fraction(rules?.plugRate).valueOf(),
                basePrice: new Fraction(rules?.basePrice).valueOf(),
                discount: new Fraction(rules?.discount).valueOf(),
            }
            const postData = {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(newRules)
            }
            await fetch(`${process.env.REACT_APP_API_BASEURL}api/Pricing/UpdateRates`, postData)
            alert("Successfully Updated Feature prices")
        }catch(err){
            alert("Error in updating rules. Check that all values are numbers")
        }
    }
    
    const divStyle = {
        display: 'grid',
        gridTemplateColumns: 'max-content 1fr',
        rowGap: '0.5rem',
        columnGap: '1rem',
        alignItems: 'center',
    }

    const handleChange = (field, value) => {
        const updatedRows = {...rules}
        updatedRows[field] = value
        setRules(updatedRows)
    }
    
    return (
        <>
            <form style={formStyle} method="post" onSubmit={handleSubmit}>
                <div style={divStyle}>
                    <label>Base Shop Rate: </label>
                    <input value={rules?.baseShopRate} onChange={(e) => handleChange('baseShopRate', e.target.value)} />
                    
                    <label>Die Cutting Shop Rate: </label>
                    <input value={rules?.dieCuttingShopRate} onChange={(e) => handleChange('dieCuttingShopRate', e.target.value)} />
                    
                    <label>Plug Rate: </label>
                    <input value={rules?.plugRate} onChange={(e) => handleChange('plugRate', e.target.value)} />
                    
                    <label>Base Price: </label>
                    <input value={rules?.basePrice} onChange={(e) => handleChange('basePrice', e.target.value)} />
                    
                    <label>Discount: ({Math.round((1 - rules?.discount) * 100)}%)</label>
                    <input value={rules?.discount} onChange={(e) => handleChange('discount', e.target.value)} />
                    
                </div>
                <button type='submit'>Apply Changes</button>
            </form>
        </>
    )
}