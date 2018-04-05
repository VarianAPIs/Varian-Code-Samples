<?xml version="1.0" encoding="utf-8"?>

<!--GenerateReport.xslt

Stylesheet to display the xml file containing the plan quality metrics.

Copyright (c) 2015 Varian Medical Systems, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.-->

<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:svg="http://www.w3.org/2000/svg">
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/PlanQualityMetrics">
    <xsl:variable name="RootNode" select="."/>
    <xsl:variable name="maxDiffInPercents">25.0</xsl:variable>
    <xsl:variable name="warningDiffInPercents">10.0</xsl:variable>
    <xsl:variable name="epsilon">0.05</xsl:variable>
    <html>
      <body>
        <h2><u>DVH estimates</u></h2>
        <img src="dvh.svg" width="750"/>

        <br></br>
        <br></br>
        <h2><u>Plan quality metrics</u></h2>
        <xsl:for-each select ="/PlanQualityMetrics/Structure">
          <h3>
            <xsl:value-of select = "@Id"/>
          </h3>
          <table border="1">
            <thead>
              <tr>
                <th>Metric</th>
                <xsl:for-each select="ClinicalPoint">
                  <xsl:choose>
                    <xsl:when test="@Type = 'VolumeAtDose'">
                      <td>
                        V<sub>
                          <xsl:value-of select="@Dose"/>
                          <xsl:value-of select="@DoseUnit"/>
                        </sub>&#160;<xsl:value-of select="@MetricType"/>&#160;<xsl:value-of select="@Target"/><xsl:value-of select="@TargetUnit"/>
                      </td>  
                    </xsl:when>
                    <xsl:otherwise>
                      <td>
                        D<sub>
                          <xsl:value-of select="@Volume"/>
                          <xsl:value-of select="@VolumeUnit"/>
                        </sub>&#160;<xsl:value-of select="@MetricType"/>&#160;<xsl:value-of select="@Target"/><xsl:value-of select="@TargetUnit"/>
                      </td>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:for-each>
              </tr>
              <tr>
                <th>Actual</th>
                <xsl:for-each select="ClinicalPoint">
                  <xsl:variable name="isSatisfied" select="@ConstraintSatisfied"/>
                  <xsl:choose>
                    <xsl:when test="$isSatisfied = 'True'">
                      <td bgcolor = "#66FF00">
                        <xsl:choose>
                          <xsl:when test="@Type = 'VolumeAtDose'">
                            <xsl:value-of select="@Volume"/>  
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="@Dose"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </td>
                    </xsl:when>
                    <xsl:otherwise>
                      <td bgcolor = "#FF000">
                        <xsl:choose>
                          <xsl:when test="@Type = 'VolumeAtDose'">
                            <xsl:value-of select="@Volume"/>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="@Dose"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </td>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:for-each>
              </tr>
            </thead>
          </table>
        </xsl:for-each>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
