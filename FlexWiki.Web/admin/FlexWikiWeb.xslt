<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes" encoding="utf-8" omit-xml-declaration="yes" standalone="no"/>
  
  <!--<xsl:template match="WomDocument">
    <xsl:element name="html">
      <xsl:if test="InputDoc">
        <xsl:element name="head">
          <xsl:apply-templates select="InputDoc" mode="head"/>
        </xsl:element>
      </xsl:if>
      <xsl:element name="body">
        <xsl:apply-templates />
      </xsl:element>
    </xsl:element>
  </xsl:template> -->
  <xsl:template match="WomDocument">
    <xsl:apply-templates />
  </xsl:template>
  

  <xsl:template match="InputDoc" mode="head">
    <xsl:element name="title">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="InputDoc"/>

  <xsl:template match="Break">
    <xsl:element name="br"/>
  </xsl:template>
  
  <xsl:template match="EmptyLine">
      <xsl:element name="p" />
  </xsl:template>

  <xsl:template match="PageRule">
    <xsl:element name="hr">
      <xsl:attribute name="class">Rule</xsl:attribute>
    </xsl:element>
  </xsl:template>  
  <!--<xsl:template match="PageRule">
    <xsl:element name="div">
      <xsl:attribute name="class">Rule</xsl:attribute>
    </xsl:element>
  </xsl:template>     -->

  <!--  No output for these elements -->
  <xsl:template match="HiddenSinglelineProperty"/>
  <xsl:template match="HiddenMultilineProperty"/>
  <xsl:template match="HiddenWikiTalkMethod"/>
  <xsl:template match="BaseEdit"/>
  <xsl:template match="BaseTopic"/>
  <xsl:template match="BaseImage"/>
  <xsl:template match="SiteUrl"/>
  <xsl:template match="WikiTalkString"/>
  <xsl:template match="HiddenExternalRef"/>
  
  <!-- This element is only output during development 
  <xsl:template match="WikiTalkString">
    <xsl:choose>
      <xsl:when test="parent::Para">
        <xsl:value-of select="."/>
      </xsl:when>
      <xsl:otherwise>
      <xsl:element name="p">
        <xsl:value-of select="."/>
      </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template> -->

  <xsl:template match="WikiStyling">
    <xsl:choose>
      <xsl:when test="count(StyleSizeBig)=2 and count(StyleColor)=1">
        <xsl:element name="big">
          <xsl:element name="big">
            <xsl:element name="span">
              <xsl:attribute name="style">
                <xsl:text>color:</xsl:text>
                <xsl:value-of select="StyleColor"/>
              </xsl:attribute>
              <xsl:apply-templates/>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:when>
      <xsl:when test="count(StyleSizeBig)=1 and count(StyleColor)=1">
        <xsl:element name="big">
          <xsl:element name="span">
            <xsl:attribute name="style">
              <xsl:text>color:</xsl:text>
              <xsl:value-of select="StyleColor"/>
            </xsl:attribute>
            <xsl:apply-templates/>
          </xsl:element>
        </xsl:element>
      </xsl:when>
      <xsl:when test="count(StyleSizeBig)=1">
        <xsl:element name="big">
          <xsl:apply-templates/>
        </xsl:element>
      </xsl:when>
      <xsl:when test="count(StyleSizeBig)=2">
        <xsl:element name="big">
          <xsl:element name="big">
            <xsl:apply-templates/>
          </xsl:element>
        </xsl:element>
      </xsl:when>
      <xsl:when test="count(StyleSizeSmall)=2 and count(StyleColor)=1">
        <xsl:element name="small">
          <xsl:element name="small">
            <xsl:element name="span">
              <xsl:attribute name="style">
                <xsl:text>color:</xsl:text>
                <xsl:value-of select="StyleColor"/>
              </xsl:attribute>
              <xsl:apply-templates/>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:when>
      <xsl:when test="count(StyleSizeSmall)=1 and count(StyleColor)=1">
        <xsl:element name="small">
          <xsl:element name="span">
            <xsl:attribute name="style">
              <xsl:text>color:</xsl:text>
              <xsl:value-of select="StyleColor"/>
            </xsl:attribute>
            <xsl:apply-templates/>
          </xsl:element>
        </xsl:element>
      </xsl:when>
      <xsl:when test="count(StyleSizeSmall)=1">
        <xsl:element name="small">
          <xsl:apply-templates/>
        </xsl:element>
      </xsl:when>
      <xsl:when test="count(StyleSizeSmall)=2">
        <xsl:element name="big">
          <xsl:element name="small">
            <xsl:apply-templates/>
          </xsl:element>
        </xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="span">
          <xsl:attribute name="style">
            <xsl:text>color:</xsl:text>
            <xsl:value-of select="StyleColor"/>
          </xsl:attribute>
          <xsl:apply-templates/>
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="StyleSizeBig"/>
  <xsl:template match="StyleSizeSmall"/>
  <xsl:template match="StyleColor"/>
  <xsl:template match="womWikiStyledText">
    <xsl:value-of select="."/>
  </xsl:template>
  
  <xsl:template match="SinglelineProperty | MultilineProperty | WikiTalkMethod">
    <xsl:element name="fieldset">
      <xsl:attribute name="class">Property</xsl:attribute>
      <xsl:attribute name="style">width: auto</xsl:attribute>
      <xsl:element name="legend">
        <xsl:attribute name="class">PropertyName</xsl:attribute>
            <xsl:element name="a">
              <xsl:attribute name="name">
                <xsl:value-of select="Name"/>
              </xsl:attribute>
              <xsl:attribute name="class">Anchor</xsl:attribute>
              <xsl:value-of select="Name"/>
            </xsl:element>
       <!-- <xsl:apply-templates select="CreateNewTopic | TopicExists"/> -->
      </xsl:element>
      <xsl:element name="div">
        <xsl:attribute name="class">PropertyValue</xsl:attribute>
      <!--  <xsl:apply-templates select="womPropertyText | wikiTalkMultiline | CreateNewTopic | TopicExists"/>  -->
        <xsl:apply-templates/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="SinglelineProperty/Name | MultilineProperty/Name | WikiTalkMethod/Name"/>
 
  
  <xsl:template match="womPropertyText | wikiTalkMultiline">
    <xsl:apply-templates/>
  </xsl:template>
  
  <xsl:template match="Header">
    <xsl:choose>
      <xsl:when test="@level='1'">
        <xsl:element name="h1">
          <xsl:if test="AnchorText">
            <xsl:attribute name="id">
              <xsl:value-of select="AnchorText"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates />
        </xsl:element>
      </xsl:when>
      <xsl:when test="@level='2'">
        <xsl:element name="h2">
          <xsl:if test="AnchorText">
            <xsl:attribute name="id">
              <xsl:value-of select="AnchorText"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates />
        </xsl:element>
      </xsl:when>
      <xsl:when test="@level='3'">
        <xsl:element name="h3">
          <xsl:if test="AnchorText">
            <xsl:attribute name="id">
              <xsl:value-of select="AnchorText"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates />
        </xsl:element>
      </xsl:when>
      <xsl:when test="@level='4'">
        <xsl:element name="h4">
          <xsl:if test="AnchorText">
            <xsl:attribute name="id">
              <xsl:value-of select="AnchorText"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates />
        </xsl:element>
      </xsl:when>
      <xsl:when test="@level='5'">
        <xsl:element name="h5">
          <xsl:if test="AnchorText">
            <xsl:attribute name="id">
              <xsl:value-of select="AnchorText"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates />
        </xsl:element>
      </xsl:when>
      <xsl:otherwise >
        <xsl:element name="h6">
          <xsl:if test="AnchorText">
            <xsl:attribute name="id">
              <xsl:value-of select="AnchorText"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates />
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="womHeaderText">
    <xsl:apply-templates />
  </xsl:template>  
  
  <!-- <xsl:template match="AnchorText">
    <xsl:if test="position() = 1">
        <xsl:attribute name="id">
          <xsl:value-of select="AnchorText"/>
        </xsl:attribute>
    </xsl:if>
  </xsl:template>   -->

<!--  <xsl:template match="womHeaderText/AnchorText"/> -->
  <xsl:template match="AnchorText"/>      

  <xsl:template match="Containerdiv">
    <xsl:element name="div">
      <xsl:if test="string-length(Id) > 0">
        <xsl:attribute name="id">
          <xsl:value-of select="Id"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:attribute name="class">
        <xsl:value-of select="Style"/>
      </xsl:attribute>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Containerdiv/Id"/>
  <xsl:template match="Containerdiv/Style"/>
  
  <xsl:template match="Containerspan">
    <xsl:element name="span">
      <xsl:if test="string-length(Id) > 0">
        <xsl:attribute name="id">
          <xsl:value-of select="Id"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:attribute name="class">
        <xsl:value-of select="Style"/>
      </xsl:attribute>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Containerspan/Id"/>
  <xsl:template match="Containerspan/Style"/>

  <xsl:template match="ErrorMessage">
    <xsl:element name="span">
      <xsl:attribute name="class">ErrorMessage</xsl:attribute>
      <xsl:if test="ErrorMessageTitle">
        <xsl:element name="span">
          <xsl:attribute name="class">ErrorMessageTitle</xsl:attribute>
          <xsl:value-of select="ErrorMessageTitle"/>
        </xsl:element>
      </xsl:if>
      <xsl:element name="span">
        <xsl:attribute name="class">ErrorMessageBody</xsl:attribute>
        <xsl:value-of select="ErrorMessageBody"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Table">
    <xsl:element name="table">
      <xsl:attribute name="class">
        <xsl:choose>
        <xsl:when test="TableRow/womCellText/TableStyle/BorderlessTable or TableRow/womMultilineCellText/TableStyle/BorderlessTable">
          <xsl:text>TableWithoutBorderClass</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>TableClass</xsl:text>
        </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:if test="count(TableRow/womCellText/TableStyle/TableCenter)=1 or count(TableRow/womMultilineCellText/TableStyle/TableCenter)=1">
        <xsl:attribute name="align">
          <xsl:text>center</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="count(TableRow/womCellText/TableStyle/TableFloatLeft)=1 or count(TableRow/womMultilineCellText/TableStyle/TableFloatLeft)=1">
        <xsl:attribute name="align">
          <xsl:text>left</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="style">;margin-left: 0; float: left</xsl:attribute>
      </xsl:if>
      <xsl:if test="count(TableRow/womCellText/TableStyle/TableFloatRight)=1 or count(TableRow/womMultilineCellText/TableStyle/TableFloatRight)=1">
        <xsl:attribute name="style">;margin-left: 0; float: right</xsl:attribute>
      </xsl:if>
      <xsl:if test="TableRow/womCellText/TableStyle/TableWidth or TableRow/womCellText/TableStyle/TableWidth">
        <xsl:attribute name="width">
          <xsl:choose>
            <xsl:when test="TableRow/womCellText/TableStyle/TableWidth">
              <xsl:value-of select="substring(TableRow/womCellText/TableStyle/TableWidth,3)"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="substring(TableRow/womMultilineCellText/TableStyle/TableWidth,3)"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:text>%</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="TableRow">
          <xsl:element name="tr">
            <xsl:apply-templates/>
          </xsl:element>
  </xsl:template>
  
  <xsl:template match="womCellText | womMultilineCellText">
    <xsl:element name="td">
      <xsl:attribute name="class">
        <xsl:choose>
          <xsl:when test="../../TableRow/womCellText/TableStyle/BorderlessTable or ../../TableRow/womMultilineCellText/TableStyle/BorderlessTable">
            <xsl:text>TableCellNoBorder</xsl:text>
          </xsl:when>
          <xsl:when test="TableStyle/CellHighlight">
            <xsl:text>TableCellHighlighted</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>TableCell</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:if test="TableStyle/StyleHexColor or TableStyle/CellNoWrap or TableStyle/CellStyleColor or TableStyle/VerticalAlign or TableStyle/StyleHexTextColor or TableStyle/CellTextColor or ../../TableRow/womCellText/TableStyle/VerticalAlign or ../../TableRow/womMultilineCellText/TableStyle/VerticalAlign">
        <xsl:attribute name="style">
          <xsl:if test="../../TableRow/womCellText/TableStyle/VerticalAlign or ../../TableRow/womMultilineCellText/TableStyle/VerticalAlign">
            <xsl:text>vertical-align: </xsl:text>
              <xsl:choose>
                <xsl:when test="../../TableRow/womCellText/TableStyle/VerticalAlign='Vt' or ../../TableRow/womMultilineCellText/TableStyle/VerticalAlign='Vt'">
                  <xsl:text>top</xsl:text>
                </xsl:when>
                <xsl:when test="../../TableRow/womCellText/TableStyle/VerticalAlign='Vm' or ../../TableRow/womMultilineCellText/TableStyle/VerticalAlign='Vm'">
                  <xsl:text>middle</xsl:text>
                </xsl:when>
                <xsl:when test="../../TableRow/womCellText/TableStyle/VerticalAlign='Vb' or ../../TableRow/womMultilineCellText/TableStyle/VerticalAlign='Vb'">
                  <xsl:text>bottom</xsl:text>
                </xsl:when>
                <xsl:when test="../../TableRow/womCellText/TableStyle/VerticalAlign='Vl' or ../../TableRow/womMultilineCellText/TableStyle/VerticalAlign='Vl'">
                  <xsl:text>baseline</xsl:text>
                </xsl:when>
              </xsl:choose>
          </xsl:if>
          <xsl:if test="TableStyle/StyleHexColor">
            <xsl:text>background: </xsl:text>
            <xsl:value-of select="TableStyle/StyleHexColor"/>
            <xsl:text>;</xsl:text>
          </xsl:if>
          <xsl:if test="TableStyle/CellStyleColor">
            <xsl:text>background: </xsl:text>
            <xsl:value-of select="TableStyle/CellStyleColor"/>
            <xsl:text>;</xsl:text>
          </xsl:if>
          <xsl:if test="TableStyle/StyleHexTextColor">
            <xsl:text>color: </xsl:text>
            <xsl:value-of select="TableStyle/StyleHexTextColor"/>
            <xsl:text>;</xsl:text>
          </xsl:if>
          <xsl:if test="TableStyle/CellTextColor">
            <xsl:text>color: </xsl:text>
            <xsl:value-of select="TableStyle/CellTextColor"/>
            <xsl:text>;</xsl:text>
          </xsl:if>
          <xsl:if test="TableStyle/CellNoWrap">
            <xsl:text>white-space: nowrap;</xsl:text>
          </xsl:if>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="TableStyle/RowSpan">
        <xsl:attribute name="rowspan">
          <xsl:value-of select="substring(TableStyle/RowSpan,2)"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="TableStyle/ColumnSpan">
        <xsl:attribute name="colspan">
          <xsl:value-of select="substring(TableStyle/ColumnSpan,2)"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="TableStyle/ColumnLeft">
        <xsl:attribute name="align">
          <xsl:text>left</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="TableStyle/ColumnCenter">
        <xsl:attribute name="align">
          <xsl:text>center</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="TableStyle/ColumnRight">
        <xsl:attribute name="align">
          <xsl:text>right</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="TableStyle/CellWidth">
        <xsl:attribute name="width">
          <xsl:value-of select="substring(TableStyle/CellWidth,2)"/>
          <xsl:text>%</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="womCell | womMultilineCell">
    <xsl:apply-templates/>
  </xsl:template>
  
  <xsl:template match="TableStyle"/>
  <xsl:template match="BorderlessTable"/>
  <xsl:template match="StyleHexColor"/>

  <xsl:template match="list[@type='ordered']">
    <xsl:element name="ol">
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="list[@type='unordered']">
    <xsl:element name="ul">
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="item">
    <xsl:element name="li">
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="Level" mode="listitem"/>
  <xsl:template match="womListText" mode="listitem">
    <xsl:element name="li">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="EscapedNoFormatText">
    <xsl:value-of select="."/>
  </xsl:template>

  <xsl:template match="ExtendedCode | PreformattedMultiline | PreformattedMultilineKeyed">
    <xsl:element name="pre">
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="PreformattedSingleLine | AltPreformattedSingleLine">
    <xsl:if test="string-length() > 0">
      <xsl:element name="pre">
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Strong | TextileStrongInLine | TextileStrongLineStart">
    <xsl:element name="strong">
     <!-- <xsl:value-of select="."/> -->
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="TextileStrong">
    <xsl:element name="strong">
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Italics">
    <xsl:element name="i">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="TextileEmphasisInLine | TextileEmphasisLineStart">
    <xsl:element name="em">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="TextileDeemphasisInLine | TextileDeemphasisLineStart">
    <xsl:element name="span">
      <xsl:attribute name="class">Deemphasis</xsl:attribute>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="TextileCitationInLine | TextileCitationLineStart">
    <xsl:element name="cite">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="TextileCodeLineInLine | TextileCodeLineStart">
    <xsl:element name="code">
      <xsl:text> </xsl:text>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="TextileDeletionInLine | TextileDeletionLineStart">
    <xsl:element name="del">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="TextileInsertedInLine | TextileInsertedLineStart">
    <xsl:element name="ins">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="TextileSubscriptInLine | TextileSubscriptLineStart">
    <xsl:element name="sub">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="TextileSuperscriptInLine | TextileSuperscriptLineStart">
    <xsl:element name="sup">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Para">
    <xsl:choose>
      <xsl:when test="ScriptData">
        <xsl:apply-templates select="ScriptData"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="p">
          <xsl:apply-templates />
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="BadNamespaceTopic">
    <xsl:value-of select="."/>
  </xsl:template>

  <xsl:template match="FreeLinkToHttpImageDisplayGif | FreeLinkToHttpsImageDisplayGif | FreeLinkToHttpImageDisplayJpg | FreeLinkToHttpsImageDisplayJpg | FreeLinkToHttpImageDisplayJpeg | FreeLinkToHttpsImageDisplayJpeg | FreeLinkToHttpImageDisplayPng | FreeLinkToHttpsImageDisplayPng">
    <xsl:apply-templates select="HttpImageDisplayGif | HttpsImageDisplayGif | HttpImageDisplayJpg | HttpsImageDisplayJpg | HttpImageDisplayJpeg | HttpsImageDisplayJpeg | HttpImageDisplayPng | HttpsImageDisplayPng"/>
    <xsl:apply-templates select="WebLink"/>
  </xsl:template>

  <xsl:template match="FreeLinkToHttpLink | FreeLinkToHttpsLink">
    <xsl:element name="a">
      <xsl:attribute name="class">externalLink</xsl:attribute>
      <xsl:attribute name="title">
        <xsl:value-of select="Tip"/>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:value-of select="HttpLink | HttpsLink"/>
      </xsl:attribute>
     <!-- <xsl:value-of select="FreeLink"/> -->
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="FreeLinkToHttpLink/HttpLink | FreeLinkToHttpsLink/HttpsLink | FreeLinkToHttpLink/Tip | FreeLinkToHttpsLink/Tip"/>

  <xsl:template match="MailtoLink">
    <xsl:element name="a">
      <xsl:attribute name="class">
        <xsl:text>externalLink</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:value-of select="."/>
      </xsl:attribute>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="FreeLinkToMailto">
    <xsl:element name="a">
      <xsl:attribute name="class">
        <xsl:text>externalLink</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:value-of select="Mailto"/>
      </xsl:attribute>
      <xsl:value-of select="FreeLinkMail"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="WebLink">
    <xsl:element name="a">
      <xsl:attribute name="class">
        <xsl:text>externalLink</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:value-of select="."/>
      </xsl:attribute>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="FreeLinkToHttpDisplayImage">
    <xsl:apply-templates select="LinkUrl"/>
  </xsl:template>
  
  <xsl:template match="LinkUrl">
    <xsl:element name="a">
      <xsl:attribute name="title">
        <xsl:value-of select="ancestor::FreeLinkToHttpDisplayImage/Title | ancestor::WikiTalkLink/Title"/>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:value-of select="."/>
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="ancestor::FreeLinkToHttpDisplayImage/ImageLink">
          <xsl:apply-templates select="ancestor::FreeLinkToHttpDisplayImage/ImageLink"/>
        </xsl:when>
        <xsl:when test="ancestor::WikiTalkLink/TextValue">
          <xsl:value-of select="ancestor::WikiTalkLink/TextValue"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="."/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <!--<xsl:template match="Title"/>-->
  
  <xsl:template match="ImageLink">
    <xsl:element name="img">
      <xsl:attribute name="src">
        <xsl:value-of select="."/>
      </xsl:attribute>
      <xsl:attribute name="border">
        <xsl:text>0</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="alt">
        <xsl:value-of select="ancestor::FreeLinkToHttpDisplayImage/Title"/>
      </xsl:attribute>
      <xsl:if test="ancestor::FreeLinkToHttpDisplayImage/ImageWidth">
        <xsl:attribute name="width">
          <xsl:value-of select="ancestor::FreeLinkToHttpDisplayImage/ImageWidth"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="ancestor::FreeLinkToHttpDisplayImage/ImageHeight">
        <xsl:attribute name="height">
          <xsl:value-of select="ancestor::FreeLinkToHttpDisplayImage/ImageHeight"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="ancestor::FreeLinkToHttpDisplayImage/Attribute">
        <xsl:for-each select="ancestor::FreeLinkToHttpDisplayImage/Attribute">
          <xsl:attribute name="{ancestor::FreeLinkToHttpDisplayImage/Attribute/AttributeName}">
            <xsl:value-of select="ancestor::FreeLinkToHttpDisplayImage/Attribute/AttributeValue"/>
          </xsl:attribute>
        </xsl:for-each>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template match="WikiTalkLink">
    <xsl:apply-templates select="LinkUrl"/>
  </xsl:template>

  <xsl:template match="HttpLink | HttpsLink | FileLink | AltFileLink ">
    <xsl:element name="a">
      <xsl:attribute name="class">
        <xsl:text>externalLink</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:value-of select="."/>
      </xsl:attribute>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="HttpDisplayImage">
    <xsl:element name="img">
      <xsl:attribute name="src">
        <xsl:value-of select="DisplayLink"/>
      </xsl:attribute>
      <xsl:attribute name="alt">
        <xsl:value-of select="Title"/>
      </xsl:attribute>
      <xsl:if test="Title">
        <xsl:attribute name="title">
          <xsl:value-of select="Title"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="ImageWidth">
        <xsl:attribute name="width">
          <xsl:value-of select="ImageWidth"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="ImageHeight">
        <xsl:attribute name="height">
          <xsl:value-of select="ImageHeight"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="Attribute">
        <xsl:if test="Attribute/AttributeValue">
          <xsl:attribute name="id">
            <xsl:value-of select="Attribute/AttributeValue"/>
          </xsl:attribute>
        </xsl:if>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template match="HttpImageDisplayGif | HttpsImageDisplayGif | HttpImageDisplayJpg | HttpsImageDisplayJpg | HttpImageDisplayJpeg | HttpsImageDisplayJpeg | HttpImageDisplayPng | HttpsImageDisplayPng">
    <xsl:element name="img">
      <xsl:attribute name ="src">
        <xsl:value-of select="."/>
      </xsl:attribute>
      <xsl:attribute name="alt">
        <xsl:value-of select="."/>
      </xsl:attribute>
    </xsl:element>
  </xsl:template>

  <xsl:template match="FreeLinkToNamespaceTopic | FreeLinkToMalformedNamespaceTopic">
    <xsl:value-of select="."/>
  </xsl:template>

  <!--  TODO: after the &amp;return= it should go to current document rather than new document-->
  <xsl:template match="CreateNamespaceTopic">
    <xsl:element name="a">
      <!-- <a title="Click here to create this topic" class="create"
      //       href="/FlexWiki/WikiEdit.aspx?topic=OdsWiki.TestBadParser&amp;return=OdsWiki.TestNewParser">TestBadParser</a> -->
      <xsl:attribute name="title">
        <xsl:text>Click here to create this topic</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>create</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:value-of select="ancestor::BaseEdit"/>
        <xsl:value-of select="NamespaceTopic | NamespaceMulticapsTopic"/>
        <xsl:text>&amp;return=</xsl:text>
        <xsl:value-of select="NamespaceTopic | NamespaceMulticapsTopic"/>
      </xsl:attribute>
      <xsl:value-of select="NamespaceTopic | NamespaceMulticapsTopic"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="CreateNamespaceTopic/CreateTopic"/>
  
  <xsl:template match="CreateNewTopic">
    <xsl:text> </xsl:text>
    <xsl:element name="a">
      <!-- <a title="Click here to create this topic" class="create"
      //       href="/FlexWiki/WikiEdit.aspx?topic=OdsWiki.TestBadParser&amp;return=OdsWiki.TestNewParser">TestBadParser</a> -->
      <xsl:attribute name="title">
        <xsl:text>Click here to create this topic</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>create</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:value-of select="ancestor::BaseEdit | preceding::BaseEdit"/>
        <xsl:value-of select="Namespace"/>
        <xsl:text>.</xsl:text>
        <xsl:value-of select="StartsWithOneCap | StartsWithMulticaps | MalformedTopic | Topic | WikiTalkMethod | SinglelineProperty"/>
        <xsl:text>&amp;return=</xsl:text>
        <xsl:value-of select="Namespace"/>
        <xsl:text>.</xsl:text>
        <xsl:value-of select="StartsWithOneCap | StartsWithMulticaps | MalformedTopic | Topic | WikiTalkMethod | SinglelineProperty"/>
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="DisplayText">
        <xsl:value-of select="DisplayText"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="StartsWithOneCap | StartsWithMulticaps | MalformedTopic"/>
      </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <!-- <a onmouseover="TopicTipOn(this, 'id573', event);" onmouseout="TopicTipOff();" href="/FlexWiki/default.aspx/OdsWiki/TestNewParser.html">TestNewParser</a>   -->
  <xsl:template match="NamespaceTopicExists | TopicExists | TopicExistsAnchor">
    <xsl:text> </xsl:text>
    <xsl:element name="a">
      <xsl:attribute name="onmouseover">
        <xsl:text>TopicTipOn(this,'</xsl:text>
        <xsl:value-of select="TipId"/>
        <xsl:text>',event);</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="onmouseout">
        <xsl:text>TopicTipOff();</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:if test="not(Anchor)">
          <xsl:value-of select="ancestor::BaseTopic | preceding::BaseTopic"/>
        </xsl:if>
        <xsl:value-of select="Namespace"/>
        <xsl:text>/</xsl:text>
        <xsl:value-of select="Topic"/>
        <xsl:text>.html</xsl:text>
        <xsl:if test="Anchor">
          <xsl:text>#</xsl:text>
          <xsl:value-of select="Anchor"/>
        </xsl:if>
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="DisplayText">
          <xsl:value-of select="DisplayText"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="Topic"/>
          <xsl:if test="Anchor">
            <xsl:text>#</xsl:text>
            <xsl:value-of select="Anchor"/>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Emoticon">
    <xsl:element name="img">
      <xsl:attribute name="src">
        <xsl:value-of select="ancestor::SiteUrl | preceding::SiteUrl"/>
        <xsl:value-of select="."/>
      </xsl:attribute>
      <xsl:attribute name="alt">
        <xsl:value-of select="."/>
      </xsl:attribute>
    </xsl:element>
  </xsl:template>

  <xsl:template match="WikiForm">
    <xsl:element name="form">
      <xsl:attribute name="action">
        <xsl:value-of select="FormAction"/>
      </xsl:attribute>
      <xsl:attribute name="method">
        <xsl:value-of select="FormMethod"/>
      </xsl:attribute>
      <xsl:for-each select="Attribute">
        <xsl:choose>
          <xsl:when test="Attribute/AttributeName">
            <xsl:attribute name="{Attribute/AttributeName}">
              <xsl:value-of select="Attribute/AttributeValue"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="id">
              <xsl:value-of select="Attribute/AttributeValue"/>
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:for-each select="FormHiddenField | FormInputBox | FormTextArea | FormSelectField | FormRadio | FormImageButton | FormSubmitButton | FormResetButton">
        <xsl:apply-templates select="."/>
        <!-- <xsl:apply-templates select="FormHiddenField"/>
        <xsl:apply-templates select="FormInputBox"/>
        <xsl:apply-templates select="FormTextarea"/>
        <xsl:apply-templates select="FormSelectField"/>
        <xsl:apply-templates select="FormRadio"/>
        <xsl:apply-templates select="FormImageButton"/>
        <xsl:apply-templates select="FormSubmitButton"/>
        <xsl:apply-templates select="FormResetButton"/>  -->
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  
  <xsl:template match="FormHiddenField">
    <xsl:element name="input">
      <xsl:attribute name="type">
        <xsl:value-of select="Type"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="Name"/>
      </xsl:attribute>
      <xsl:if test="Attribute">
        <xsl:if test="Attribute/AttributeName">
          <xsl:attribute name="{Attribute/AttributeName}">
            <xsl:value-of select="Attribute/AttributeValue"/>
          </xsl:attribute>
        </xsl:if>
      </xsl:if>
      <xsl:attribute name="value">
        <xsl:value-of select="Value"/>
      </xsl:attribute>
    </xsl:element>
  </xsl:template>

  <xsl:template match="FormInputBox">
    <xsl:element name="input">
      <xsl:attribute name="type">
        <xsl:value-of select="Type"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="Name"/>
      </xsl:attribute>
      <xsl:attribute name="id">
        <xsl:value-of select="Id"/>
      </xsl:attribute>
      <xsl:attribute name="value">
        <xsl:value-of select="Value"/>
      </xsl:attribute>
      <xsl:if test="Size">
        <xsl:attribute name="size">
          <xsl:value-of select="Size"/>
        </xsl:attribute>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template match="FormImageButton">
    <xsl:element name="input">
      <xsl:attribute name="type">
        <xsl:value-of select="Type"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="Source"/>
      </xsl:attribute>
      <xsl:attribute name="title">
        <xsl:value-of select="Title"/>
      </xsl:attribute>
      <xsl:if test="Attribute">
        <xsl:if test="Attribute/AttributeName">
          <xsl:attribute name="{Attribute/AttributeName}">
            <xsl:value-of select="Attribute/AttributeValue"/>
          </xsl:attribute>
        </xsl:if>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template match="ScriptData">
    <xsl:choose>
      <xsl:when test="script">
        <xsl:for-each select="script">
          <xsl:choose>
            <xsl:when test="@src">
              <xsl:element name="script">
                <xsl:attribute name="type">
                  <xsl:value-of select="@type"/>
                </xsl:attribute>
                <xsl:attribute name="src">
                  <xsl:value-of select="@src"/>
                </xsl:attribute>
                <xsl:text>&#160;</xsl:text>
              </xsl:element>
            </xsl:when>
            <xsl:otherwise>
              <xsl:element name="script">
                <xsl:attribute name="type">
                  <xsl:value-of select="@type"/>
                </xsl:attribute>
                <xsl:if test="child::node()">
                  <xsl:text disable-output-escaping="yes">&#x3c;!--</xsl:text>
                  <xsl:value-of select="child::node()"/>
                  <xsl:text disable-output-escaping="yes">--&#x3e;</xsl:text>
                </xsl:if>
              </xsl:element>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of disable-output-escaping="yes" select="child::node()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="script"/>

  <xsl:template match="ExternalWikiLink">
    <xsl:copy-of select="a"/>
  </xsl:template>

  <xsl:template match="FormSelectField">
    <xsl:element name="select">
      <xsl:attribute name="name">
        <xsl:value-of select="Name"/>
      </xsl:attribute>
      <xsl:attribute name="id">
        <xsl:value-of select="Id"/>
      </xsl:attribute>
      <xsl:attribute name="size">
        <xsl:value-of select="Size"/>
      </xsl:attribute>
      <xsl:for-each select="SelectOption">
        <xsl:element name="option">
          <xsl:if test="Value">
            <xsl:attribute name="value">
              <xsl:value-of select="Value"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="Selected">
            <xsl:attribute name="selected">selected</xsl:attribute>
          </xsl:if>
          <xsl:value-of select="OptionString"/>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template match="FormSubmitButton">
    <xsl:element name="button">
      <xsl:attribute name="type">
        <xsl:value-of select="Type"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="Name"/>
      </xsl:attribute>
      <xsl:attribute name="value">
        <xsl:value-of select="Value"/>
      </xsl:attribute>
      <xsl:attribute name="id">
        <xsl:value-of select="Id"/>
      </xsl:attribute>
      <xsl:value-of select="Value"/>
    </xsl:element>
  </xsl:template>

  <!-- <div id="id573" style="display: none">
    //      verify behavior of certain conditions for replication or correction in the new parser
    //      <div class="TopicTipStats">5/22/2008 12:06:58 PM - -76.70.99.195</div>
  </div>
  _intermediate.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}>", "NamespaceTopicExists", "Namespace", testitem[0], "Topic", testitem[1], "TipId", tipid);
    _intermediate.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}><{1}></{0}>", "NamespaceTopicExists", "TipData", "TipIdData", tipid, "TipText", tiptext, "TipStat", tipstat); -->
  <xsl:template match="TipHolder">
    <xsl:apply-templates select="preceding::NamespaceTopicExists/TipData | preceding::TopicExists/TipData | preceding::TopicExistsAnchor/TipData" mode="EndTopic"/>
  </xsl:template>

  <xsl:template match="TipData" mode="EndTopic">
    <xsl:element name="div">
      <xsl:attribute name="id">
        <xsl:value-of select="TipIdData"/>
      </xsl:attribute>
      <xsl:attribute name="style">
        <xsl:text>display: none</xsl:text>
      </xsl:attribute>
      <xsl:value-of select="TipText"/>
      <xsl:element name="div">
        <xsl:attribute name="class">
          <xsl:text>TopicTipStats</xsl:text>
        </xsl:attribute>
        <xsl:value-of select="TipStat"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>

