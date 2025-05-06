import React from "react";

const Contact = () => (
    <div className="info-section">
        <div className="info-item">
            <div className="icon-circle">
                <i className="fa-solid fa-phone"></i>
            </div>
            <div className="info-text">
                <h4>Customer Support</h4>
                <p>(800) 899-3437</p>
            </div>
        </div>

        <div className="info-item">
            <div className="icon-circle">
                <i className="fa-solid fa-location-dot"></i>
            </div>
            <div className="info-text">
                <h4>Our Location</h4>
                <p>391 Malden Street</p>
                <p>South St. Paul, MN 55075</p>
            </div>
        </div>
    </div>
);

export default Contact;
