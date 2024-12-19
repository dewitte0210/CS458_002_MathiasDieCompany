This file is for documentation of Ellipse.cs that cannot be explained properly in comments.

# fullPerimeterCalc()
The full perimeter calculation aproximates the perimeter of the ellipse using the arithmetic-geometric mean (AGM) method. This method takes two numbers and finds the
convergence limit of their arithmetic and geometric means iteratively. Let $n$ be the iteration step number, $a$ be the arithmetic mean, $g$ be the geometric mean, 
$A$ be the semi-major axis, and $B$ be the semi-minor axis.  
The intial arithmetic and geometric mean are calculated as follows:

### $a_0 = 1$
### $g_0 = B/A$

Iteratively, the sum is then calculated as follows:

### $a_{n+1} = \frac{a_n + g_n}{2}$
### $g_{n+1} = \sqrt{a_n \cdot g_n}$

### $\text{Sum} \text{ += } 2^n (a^{2}_n - g^{2}_n)$
\
\
After all iterations, the perimeter is calculated as follows:

### $P \approx \frac{4A\pi}{2a_n} (1-\text{total})$  
<br><br/>
# partialPerimeterCalc()
numLines is calculated to scale by the perimeter size of the full ellipse. A "Unit Ellipse" is used as a baseline ellipse for this calculation.

## Unit Ellipse
### $\frac{x^2}{4}+\frac{y^2}{1}=1$
<br><br/>
A 3D regression was made on a set of data points in the form (semi-major axis, semi-minor axis, full perimeter estimation). The values of the semi-major and semi-minor axis' 
ranged from 1-10, resulting in 17 plots on a 3D graph. The type of regression used was polynomial regression (degree 3) with a $R^2$ value of 0.99999. The resulting polynomial is:

### $P(a,b)=-0.0113a^3 + 0.0072a^2 b + 0.0075ab^2 + 0.3639a^2 + 0.0104b^3 + 0.4526ab + 1.5975a + 2.0306b + 3.7996$

The partial derivatives of $a$ and $b$ are performed on the regression to find the change in perimeter given by the following formula:

### $dP= \frac{\delta P}{\delta a}da + \frac{\delta P}{\delta b}db$

where

### $\frac{\delta P}{\delta a} = -0.0339a^2 + 0.0144ab + 0.0075b^2 + 0.7278a + 0.4526b + 1.5975$  
### $\frac{\delta P}{\delta b} = 0.0072a^2 + 0.015ab + 0.0312b^2 + 0.4526a + 2.0306$

Substituting the two partial derivatives in the $dP$ equation, we have the variables $a\text{, } b\text{, } da\text{, and } db$ to work with. Using the unit ellipse
as a baseline, values for each variable can be calculated as:

### $a=2$  
### $b=1$  
### $da=\text{this.major}-2$  
### $db=\text{this.minor}-1$
\
\
$dP$ then becomes:

### $dp=3.4064da + 3.0258db$
\
\
As for the number of lines at the unit ellipse, 360 is used which gives 4 decimal places of precision in the perimeter calculation.
