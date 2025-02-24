﻿import React, {useEffect, useRef} from 'react';

interface VisualDisplayProps {
    touchingEntities: any[];
    minX:number;
    maxX:number;
    minY:number;
    maxY:number;
}

const VisualDisplay: React.FC<VisualDisplayProps> = ({touchingEntities, minX, maxX, minY, maxY}) => {
    // Reference to the canvas element
    const canvasRef = useRef<HTMLCanvasElement>(null);

    // useEffect hook is used to handle the drawing logic when the component mounts or when drawData or scaleFactor changes
    useEffect(() => {
        const canvas = canvasRef.current;
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        if (!ctx) return;

        // Function to draw all shapes
        const drawCanvas = () => {
            // Clear the canvas before redrawing
            ctx.clearRect(0, 0, canvas.width, canvas.height);

            // Padding to ensure the shapes aren't clipped
            let padding = 0.1;

            minX -= padding;
            minY -= padding;
            maxX += padding;
            maxY += padding;
            
            console.log(minX, minY, maxX, maxY);

            // Calculate scaling factor to fit all shapes within the canvas
            const scaleFactor = Math.abs(600 / Math.max(maxX - minX, maxY - minY));

            // Set canvas dimensions based on the bounding box
            canvas.width = (maxX - minX) * scaleFactor;
            canvas.height = (maxY - minY) * scaleFactor;

            // Center the canvas visually
            canvas.style.display = 'block';
            canvas.style.margin = '0 auto';

            // Flip y-axis (canvas coordinates vs. typical 2D Cartesian coordinates)
            ctx.setTransform(1, 0, 0, -1, 0, canvas.height);

            // Outline the canvas with a dashed border for clarity
            ctx.setLineDash([5, 10]);
            ctx.strokeRect(0, 0, canvas.width, canvas.height);
            ctx.setLineDash([]); // Reset to solid lines

            // Calculate offsets to shift shapes into the visible area
            let xOffset = Math.abs(Math.min(0, minX));
            let yOffset = Math.abs(Math.min(0, minY));

            // Iterate over each shape and draw it on the canvas
            touchingEntities.forEach((object) => {
                object.forEach((shape) => {
                    if (shape["$type"].includes('Line')) {
                        drawLine(ctx, shape, scaleFactor, xOffset, yOffset);
                    } else if (shape["$type"].includes('Arc')) {
                        drawArc(ctx, shape, scaleFactor, xOffset, yOffset);
                    } else if ( shape["$type"].includes('Circle')) {
                        drawCircle(ctx, shape, scaleFactor, xOffset, yOffset);
                    } //else if (shape["$type"].includes() === 'quadcurve2d') {
                    //     drawQuadLine(ctx, shape, scaleFactor, xOffset, yOffset);
                    // } else if (shape["$type"].includes() === 'cubiccurve2d') {
                    //     drawCubicLine(ctx, shape, scaleFactor, xOffset, yOffset);
                    // }
                });
            });
        };


        const drawLine = (ctx, shape, scaleFactor, xOffset, yOffset) => {
            ctx.beginPath();
            const startPoint = shape.StartPoint
            const endPoint = shape.EndPoint
            ctx.moveTo((startPoint.X + xOffset) * scaleFactor, (startPoint.Y + yOffset) * scaleFactor);
            ctx.lineTo((endPoint.X + xOffset) * scaleFactor, (endPoint.Y + yOffset) * scaleFactor);

            // Sets the color of the line based on whether it is a kiss cut (green) or not (black)
            ctx.strokeStyle = 'black';
            ctx.setLineDash([]);
            ctx.lineWidth = 2;
            ctx.stroke();
        };

        const drawArc = (ctx, shape, scaleFactor, xOffset, yOffset) => {
            ctx.beginPath();
            const center = shape.Center;
            ctx.arc(
                (center.X + xOffset) * scaleFactor,
                (center.Y + yOffset) * scaleFactor,
                shape.Radius * scaleFactor,
                shape.EndAngle * (Math.PI / 180),
                shape.StartAngle * (Math.PI / 180),
                true
            );
            // Sets the color of the arc based on whether it is a kiss cut (green) or not (red)
            ctx.strokeStyle = 'black';
            ctx.setLineDash([]);
            ctx.lineWidth = 2;
            ctx.stroke();
        };

        const drawCircle = (ctx, shape, scaleFactor, xOffset, yOffset) => {
            ctx.beginPath();
            const center = shape.Center;
            ctx.arc(
                (center.X + xOffset) * scaleFactor,
                (center.Y + yOffset) * scaleFactor,
                shape.Radius * scaleFactor,
                0,
                2 * Math.PI
            );
            // Sets the color of the circle based on whether it is a kiss cut (green) or not (blue)
            ctx.strokeStyle = 'black';
            ctx.setLineDash([]);
            ctx.lineWidth = 2;
            ctx.stroke();
        };

        const drawQuadLine = (ctx, shape, scaleFactor, xOffset, yOffset) => {
            ctx.beginPath();
            const startX = (shape.startX + xOffset) * scaleFactor;
            const startY = (shape.startY + yOffset) * scaleFactor;
            ctx.moveTo(startX, startY);

            const ctrlX = (shape.controlX + xOffset) * scaleFactor;
            const ctrlY = (shape.controlY + yOffset) * scaleFactor;

            const endX = (shape.endX + xOffset) * scaleFactor;
            const endY = (shape.endY + yOffset) * scaleFactor;

            ctx.quadraticCurveTo(ctrlX, ctrlY, endX, endY);
            ctx.strokeStyle = 'purple';
            ctx.setLineDash([]);
            ctx.lineWidth = 2; // Set line width
            ctx.stroke();
        };

        const drawCubicLine = (ctx, shape, scaleFactor, xOffset, yOffset) => {
            ctx.beginPath();
            const startX = (shape.startX + xOffset) * scaleFactor;
            const startY = (shape.startY + yOffset) * scaleFactor;
            ctx.moveTo(startX, startY);

            const ctrl1X = (shape.control1X + xOffset) * scaleFactor;
            const ctrl1Y = (shape.control1Y + yOffset) * scaleFactor;

            const ctrl2X = (shape.control2X + xOffset) * scaleFactor;
            const ctrl2Y = (shape.control2Y + yOffset) * scaleFactor;

            const endX = (shape.endX + xOffset) * scaleFactor;
            const endY = (shape.endY + yOffset) * scaleFactor;

            ctx.bezierCurveTo(ctrl1X, ctrl1Y, ctrl2X, ctrl2Y, endX, endY);
            ctx.strokeStyle = 'orange';
            ctx.setLineDash([]);
            ctx.lineWidth = 2; // Set line width
            ctx.stroke();
        };
        drawCanvas();
    }, [touchingEntities]);

    // Return the canvas element
    return (
        <div id="container">
            <canvas ref={canvasRef} width={window.innerWidth} height={window.innerHeight}/>
        </div>
    );
};

export default VisualDisplay;
