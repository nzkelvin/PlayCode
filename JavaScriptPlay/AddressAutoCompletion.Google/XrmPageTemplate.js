// Build the Xrm object ==============================================================
var Xrm = {
    Page: {
        context: new _context(),
        data: {
            entity: new Entity()
        },
        ui: {
            controls: new _uiControlsMethods(),
            formSelector: new _formSelector(),
            navigation: new _navigation(),
            tabs: new _tabsMethods(),
            close: function () {
                /// <summary>
                /// Closes the form.
                /// </summary>
            },
            getCurrentControl: function () {
                /// <summary>
                /// Returns the control object that currently has focus on the form.
                /// </summary>
                /// <returns type="Object" />
                return Xrm.Page.ui.controls.get(this.z_currentControl);
            },
            getFormType: function () {
                /// <summary>
                /// Indicates the form context for the record.
                /// <para>    0 : Undefined</para>
                /// <para>    1 : Create</para>
                /// <para>    2 : Update</para>
                /// <para>    3 : Read Only</para>
                /// <para>    4 : Disabled</para>
                /// <para>    5 : Quick Create (Deprecated)</para>
                /// <para>    6 : Bulk Edit</para>
                /// <para>    11 : Read Optimized</para>
                /// </summary>
                /// <returns type="Number" />
                return pageData.FormType;
            },
            getViewPortHeight: function () {
                /// <summary>
                /// Returns the height of the viewport in pixels.
                /// </summary>
                /// <returns type="Number" />
                return pageData.ViewPortHeight;
            },
            getViewPortWidth: function () {
                /// <summary>
                /// Returns the width of the viewport in pixels.
                /// </summary>
                /// <returns type="Number" />
                return pageData.ViewPortWidth;
            },
            refreshRibbon: function () {
                /// <summary>
                /// Causes the ribbon to re-evaluate data that controls what is displayed in it.
                /// </summary>
            }
        },
        getAttribute: function (attributeName) {

        }
    }
};

// ENTITY =====================================================

function Entity() {

}
Entity.prototype.save = function () { };

function _entity() {
    return {
        addOnSave: function (functionPointer) {
            /// <summary>
            /// Adds the event handler function to be called when the entity is saved.
            /// It will be added to the bottom of the event pipeline and called after the other event handlers.
            /// </summary>
            this.z_saveEventHandlers.push(functionPointer);
        },
        removeOnSave: function (functionPointer) {
            /// <summary>
            /// Removes the the event handler function from the event pipeline.
            /// </summary>
            var newSaveHandlers = [];
            for (var i = 0; i < this.z_saveEventHandlers.length; i++) {
                if (this.z_saveEventHandlers[i] != functionPointer)
                { newSaveHandlers.push(this.z_saveEventHandlers[i]); }
            }
            this.z_saveEventHandlers = newSaveHandlers;
        },
        getDataXml: function () {
            /// <summary>
            /// Returns a string representing the xml that will be sent to the server when the record is saved. Only data in fields that have changed are set to the server.
            /// </summary>
            /// <returns type="String" />
            var entityName = Xrm.Page.data.entity.getEntityName();
            var returnString = "<" + entityName + ">";
            for (var i = 0; i < Xrm.Page.data.entity.z_attCol.length; i++) {

                var attribute = Xrm.Page.getAttribute(i);
                var attributeName = attribute.getName();
                var attributeValue = attribute.getValue();

                if ((attribute.getSubmitMode() == "always") || (attribute.getIsDirty())) {
                    if (attribute.getValue() == null) {
                        returnString += "<" + attributeName + "/>";
                    }
                    else {
                        returnString += "<" + attributeName + ">" + attributeValue + "</" + attributeName + ">";
                    }

                }
            }

            returnString += "</" + entityName + ">";

            return returnString



        },
        getEntityName: function () {
            /// <summary>
            /// Returns a string representing the logical name of the entity for the record.
            /// </summary>
            /// <returns type="String" />
            return pageData.EntityName;
        },
        getId: function () {
            /// <summary>
            /// Returns a string representing the GUID id value for the record.
            /// </summary>
            /// <returns type="String" />
            return pageData.Id;
        },
        getIsDirty: function () {
            /// <summary>
            /// Returns a Boolean value that indicates if any fields in the form have been modified.
            /// </summary>
            /// <returns type="Boolean" />
            return this.z_isDirty;
        },
        save: function (argument) {
            /// <summary>
            /// Saves the record.
            /// </summary>
            /// <param name="argument" type="String" mayBeNull="true" optional="true" >
            /// <para>1: None: If no parameter is included the record will simply be saved. </para>
            /// <para>2: "saveandclose" : Saves record and closes the form.</para>  
            /// <para>3: "saveandnew" : Saves the record and opens a blank form for a new record.</para>
            /// </param>
            for (var i = 0; i < this.z_saveEventHandlers.length; i++) {
                this.z_saveEventHandlers[i]();
            }
            if (this.z_defaultPrevented == false) {
                this.z_isDirty = false;
                var numberOfAttributes = this.attributes.getLength();
                for (var i = 0; i < numberOfAttributes; i++) {
                    this.attributes.get(i).z_isDirty = false;
                }
            }

        },
        z_attCol: new _attributes(),
        attributes: new _AttributesMethods()
    };
}