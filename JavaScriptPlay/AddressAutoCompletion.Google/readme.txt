------------Files part of the crm solution-----------
AddressSuggestion.js
AddressSuggestionBox.html
Kelvin.Common.js

------------Files for js intellisense-----------
XrmPageTemplate.js
XrmPageTemplate2011.js

------------Sample web resource custom configuration value------------
{"googlekey": "AIzaSyArNPkX_fmd95h40NPjfKPfwXvIlwmn37U","map": {"street_number": "address1_line1","route": "address1_line2","sublocality_level_1": "address1_line3","locality": "address1_city","administrative_area_level_1": "address1_stateorprovince","country": "address1_country","postal_code": "address1_postalcode"}}

------------Formatted sample web resource custom configuration value (Don't use)------------
{
   "googlekey":"AIzaSyArNPkX_fmd95h40NPjfKPfwXvIlwmn37U",
   "map":{
		"street_number": "address1_line1",
		"route": "address1_line2",
		"sublocality_level_1": "address1_line3",
		"locality": "address1_city",
		"administrative_area_level_1": "address1_stateorprovince",
		"country": "address1_country",
		"postal_code": "address1_postalcode"
	}
}

Explain: googlekey is option as long as you stay within google's free requests quota.
Explain 2: map is very important. It maps properties of google place api response to crm attributes and also to html controls in the web resource.