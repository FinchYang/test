MFAPI = { 

	getPrototype: function(mfTypeName) {
		var mfType = this[mfTypeName];

		if(!mfType) return;
		
		if(!mfType.proto) {
			var def = {
				constructor: { enumerable:true, value: mfTypeName },
				toString: { value: function() {
					var that = this,
						str = mfTypeName + "{ \n";
					$.each(mfType.properties, function(n,t) {
						str += "\t" + n + ": ";
						try {
							if( MFAPI[t] ) {
								str += t;
							} else {
								str += that[n];
							}
						}catch(e){
							str += "ERROR!";
						}
						
						str += "\n";
					});
					
					return str;
				} }
			};
			$.each(mfType.properties, function(name,type) {
				def[name] = {
					enumerable:true,
					get: function() { return MFAPI.wrap(this.MFO[name], type); },
					set: function(v) { this.MFO[name] = MFAPI.unwrap(v); }
				};
			});
			mfType.proto = Object.create({}, def);
		}
		return mfType.proto
	},
	
	wrap: function(mfObj, mfTypeName) {
		if( mfTypeName.indexOf(".") ) {
			mfTypeName = mfTypeName.split(".").pop();
		}
		var proto = this.getPrototype(mfTypeName);
			
		if(!mfObj || !proto) {
			return mfObj;
		}
		
		return Object.create(proto, {
			MFO: {value: mfObj, enumerable:true}
		});
	},
	
	unwrap: function(obj) {
		return obj.MFO || obj;
	},


/// AUTO-GENERATED M-FILES TYPE INFO BELOW

	'AccessControlList': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.AccessControlEntry',
			'CustomComponent': 'MFilesAPI.AccessControlListComponent',
			'AutomaticComponents': 'MFilesAPI.AccessControlListComponentContainer',
			'CheckedOutToUserID': 'System.Int32',
			'HasCheckedOutToUserID': 'System.Boolean',
			'IsFullyAuthoritative': 'System.Boolean'
		},
		methods: {
			'EqualTo': 'System.Boolean',
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'GetACEByUserOrGroupID': 'MFilesAPI.AccessControlEntry',
			'GetACEIndexByUserOrGroupID': 'System.Int32',
			'Clone': 'MFilesAPI.AccessControlList',
			'HasIdenticalPermissions': 'System.Boolean'
		}
	},
	'AccessControlEntry': {
		properties: {
			'UserOrGroupID': 'System.Int32',
			'IsGroup': 'System.Boolean',
			'ReadPermission': 'MFilesAPI.MFPermission',
			'EditPermission': 'MFilesAPI.MFPermission',
			'ChangePermissionsPermission': 'MFilesAPI.MFPermission'
		},
		methods: {
			'Clone': 'MFilesAPI.AccessControlEntry'
		}
	},
	'AccessControlListComponent': {
		properties: {
			'AccessControlEntries': 'MFilesAPI.AccessControlEntryContainer',
			'IsActive': 'System.Boolean',
			'HasCurrentUser': 'System.Boolean',
			'HasPseudoUsers': 'System.Boolean',
			'CanDeactivate': 'System.Boolean',
			'NamedACLLink': 'System.Int32',
			'CurrentUserBinding': 'System.Int32',
			'HasNamedACLLink': 'System.Boolean',
			'HasCurrentUserBinding': 'System.Boolean'
		},
		methods: {
			'GetACEByUserOrGroupID': 'MFilesAPI.AccessControlEntryData',
			'GetACEKeyByUserOrGroupID': 'MFilesAPI.AccessControlEntryKey',
			'ResetNamedACLLink': 'System.Void',
			'ResetCurrentUserBinding': 'System.Void',
			'Clone': 'MFilesAPI.AccessControlListComponent'
		}
	},
	'AccessControlEntryContainer': {
		properties: {
			'IsEmpty': 'System.Boolean'
		},
		methods: {
			'GetKeys': 'MFilesAPI.AccessControlEntryKeys',
			'HasKey': 'System.Boolean',
			'At': 'MFilesAPI.AccessControlEntryData',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clear': 'System.Void',
			'Clone': 'MFilesAPI.AccessControlEntryContainer'
		}
	},
	'AccessControlEntryKeys': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.AccessControlEntryKey'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Clone': 'MFilesAPI.AccessControlEntryKeys'
		}
	},
	'AccessControlEntryKey': {
		properties: {
			'UserOrGroupID': 'System.Int32',
			'IsGroup': 'System.Boolean',
			'IsPseudoUser': 'System.Boolean',
			'HasConcreteUserOrGroupID': 'System.Boolean',
			'PseudoUserID': 'MFilesAPI.IndirectPropertyID'
		},
		methods: {
			'SetUserOrGroupID': 'System.Void',
			'GetResolvedPseudoUserOrGroupIDs': 'MFilesAPI.UserOrUserGroupIDs',
			'ResetResolvedPseudoUserOrGroupIDs': 'System.Void',
			'Clone': 'MFilesAPI.AccessControlEntryKey'
		}
	},
	'IndirectPropertyID': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.IndirectPropertyIDLevel'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'EqualTo': 'System.Boolean',
			'Clone': 'MFilesAPI.IndirectPropertyID'
		}
	},
	'IndirectPropertyIDLevel': {
		properties: {
			'LevelType': 'MFilesAPI.MFIndirectPropertyIDLevelType',
			'ID': 'System.Int32'
		},
		methods: {
			'Clone': 'MFilesAPI.IndirectPropertyIDLevel'
		}
	},
	'UserOrUserGroupIDs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.UserOrUserGroupID'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'GetUserOrUserGroupID': 'MFilesAPI.UserOrUserGroupID',
			'GetUserOrUserGroupIDIndex': 'System.Int32',
			'Clone': 'MFilesAPI.UserOrUserGroupIDs'
		}
	},
	'UserOrUserGroupID': {
		properties: {
			'UserOrGroupType': 'MFilesAPI.MFUserOrUserGroupType',
			'UserOrGroupID': 'System.Int32'
		},
		methods: {
			'Clone': 'MFilesAPI.UserOrUserGroupID'
		}
	},
	'AccessControlEntryData': {
		properties: {
			'ReadPermission': 'MFilesAPI.MFPermission',
			'EditPermission': 'MFilesAPI.MFPermission',
			'DeletePermission': 'MFilesAPI.MFPermission',
			'ChangePermissionsPermission': 'MFilesAPI.MFPermission'
		},
		methods: {
			'SetAllPermissions': 'System.Void',
			'Clone': 'MFilesAPI.AccessControlEntryData'
		}
	},
	'AccessControlListComponentContainer': {
		properties: {
			'Count': 'System.Int32'
		},
		methods: {
			'GetKeys': 'MFilesAPI.AccessControlListComponentKeys',
			'HasKey': 'System.Boolean',
			'At': 'MFilesAPI.AccessControlListComponent'
		}
	},
	'AccessControlListComponentKeys': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.AccessControlListComponentKey'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Clone': 'MFilesAPI.AccessControlListComponentKeys'
		}
	},
	'AccessControlListComponentKey': {
		properties: {
			'PropertyDefID': 'System.Int32',
			'ItemID': 'System.Int32'
		},
		methods: {
			'Clone': 'MFilesAPI.AccessControlListComponentKey'
		}
	},
	'AutomaticPermissions': {
		properties: {
			'NamedACL': 'MFilesAPI.NamedACL',
			'IsDefault': 'System.Boolean',
			'IsBasedOnObjectACL': 'System.Boolean',
			'CanDeactivate': 'System.Boolean'
		},
		methods: {
			'SetNamedACL': 'System.Void',
			'SetBasedOnObjectACL': 'System.Void',
			'Clone': 'MFilesAPI.AutomaticPermissions'
		}
	},
	'NamedACL': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String',
			'AccessControlList': 'MFilesAPI.AccessControlList',
			'AccessControlListForNamedACL': 'MFilesAPI.AccessControlList',
			'NamedACLType': 'MFilesAPI.MFNamedACLType'
		},
		methods: {
			'Clone': 'MFilesAPI.NamedACL'
		}
	},
	'ObjID': {
		properties: {
			'Type': 'System.Int32',
			'ID': 'System.Int32'
		},
		methods: {
			'SetIDs': 'System.Void',
			'Clone': 'MFilesAPI.ObjID',
			'Serialize': 'System.Byte[]',
			'Unserialize': 'System.Void'
		}
	},
	'ObjIDs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjID'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void'
		}
	},
	'ObjVer': {
		properties: {
			'Type': 'System.Int32',
			'ID': 'System.Int32',
			'ObjID': 'MFilesAPI.ObjID',
			'Version': 'System.Int32'
		},
		methods: {
			'SetIDs': 'System.Void',
			'SetObjIDAndVersion': 'System.Void',
			'Clone': 'MFilesAPI.ObjVer',
			'Serialize': 'System.Byte[]',
			'Unserialize': 'System.Void'
		}
	},
	'VaultEventLogOperations': {
		properties: {

		},
		methods: {
			'Clear': 'System.Void',
			'IsLoggingEnabled': 'System.Boolean',
			'SetLoggingEnabled': 'System.Void',
			'ExportAll': 'System.String',
			'ExportRange': 'System.String',
			'ClearRange': 'System.Void',
			'ExportRange_32bit': 'System.String',
			'ClearRange_32bit': 'System.Void'
		}
	},
	'FileVer': {
		properties: {
			'ID': 'System.Int32',
			'Version': 'System.Int32'
		},
		methods: {
			'Clone': 'MFilesAPI.FileVer'
		}
	},
	'ObjOrFileVer': {
		properties: {
			'ObjVer': 'MFilesAPI.ObjVer',
			'FileVer': 'MFilesAPI.FileVer'
		},
		methods: {
			'IsFile': 'System.Boolean'
		}
	},
	'ObjectClass': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String',
			'AssociatedPropertyDefs': 'MFilesAPI.AssociatedPropertyDefs',
			'Workflow': 'System.Int32',
			'ObjectType': 'System.Int32',
			'ACLForObjects': 'MFilesAPI.AccessControlList',
			'NamePropertyDef': 'System.Int32',
			'AutomaticPermissionsForObjects': 'MFilesAPI.AutomaticPermissions',
			'ForceWorkflow': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.ObjectClass'
		}
	},
	'AssociatedPropertyDefs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.AssociatedPropertyDef'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.AssociatedPropertyDefs'
		}
	},
	'AssociatedPropertyDef': {
		properties: {
			'PropertyDef': 'System.Int32',
			'Required': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.AssociatedPropertyDef'
		}
	},
	'ObjectClassAdmin': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String',
			'AssociatedPropertyDefs': 'MFilesAPI.AssociatedPropertyDefs',
			'Workflow': 'System.Int32',
			'ObjectType': 'System.Int32',
			'ACLForObjects': 'MFilesAPI.AccessControlList',
			'NamePropertyDef': 'System.Int32',
			'ForceWorkflow': 'System.Boolean',
			'Predefined': 'System.Boolean',
			'AutomaticPermissionsForObjects': 'MFilesAPI.AutomaticPermissions',
			'SemanticAliases': 'MFilesAPI.SemanticAliases'
		},
		methods: {
			'Clone': 'MFilesAPI.ObjectClassAdmin'
		}
	},
	'SemanticAliases': {
		properties: {
			'Value': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.SemanticAliases'
		}
	},
	'ObjectClasses': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjectClass'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.ObjectClasses'
		}
	},
	'ObjectClassesAdmin': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjectClassAdmin'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.ObjectClassesAdmin'
		}
	},
	'ObjectFile': {
		properties: {
			'ID': 'System.Int32',
			'Version': 'System.Int32',
			'Title': 'System.String',
			'Extension': 'System.String',
			'LogicalSize': 'System.Int64',
			'LogicalSize_32bit': 'System.Int32',
			'CreationTimeUtc': 'System.DateTime',
			'LastAccessTimeUtc': 'System.DateTime',
			'LastWriteTimeUtc': 'System.DateTime',
			'ChangeTimeUtc': 'System.DateTime',
			'FileVer': 'MFilesAPI.FileVer',
			'FileGUID': 'System.String'
		},
		methods: {
			'GetNameForFileSystem': 'System.String'
		}
	},
	'ObjectType': {
		properties: {
			'ID': 'System.Int32',
			'NameSingular': 'System.String',
			'NamePlural': 'System.String',
			'Icon': 'System.Byte[]',
			'RealObjectType': 'System.Boolean',
			'Hierarchical': 'System.Boolean',
			'HasOwnerType': 'System.Boolean',
			'OwnerType': 'System.Int32',
			'OwnerPropertyDef': 'System.Int32',
			'CanHaveFiles': 'System.Boolean',
			'AllowAdding': 'System.Boolean',
			'External': 'System.Boolean',
			'ShowCreationCommandInTaskPane': 'System.Boolean',
			'DefaultPropertyDef': 'System.Int32',
			'AccessControlList': 'MFilesAPI.AccessControlList',
			'ReadOnlyPropertiesDuringInsert': 'MFilesAPI.IDs',
			'ReadOnlyPropertiesDuringUpdate': 'MFilesAPI.IDs',
			'ObjectTypeTargetsForBrowsing': 'MFilesAPI.ObjectTypeTargetsForBrowsing',
			'DefaultAccessControlList': 'MFilesAPI.AccessControlList',
			'SupportsStartsWithAtWordBoundarySearches': 'System.Boolean'
		},
		methods: {
			'GetIconAsPNG': 'System.Byte[]',
			'Clone': 'MFilesAPI.ObjType',
			'CanHaveItemIcons': 'System.Boolean'
		}
	},
	'Ds': {
		properties: {

		},
		methods: {

		}
	},
	'IDs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'System.Int32'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.IDs',
			'IndexOf': 'System.Int32',
			'RemoveAll': 'System.Int32'
		}
	},
	'ObjectTypeTargetsForBrowsing': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjectTypeTargetForBrowsing'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.ObjectTypeTargetsForBrowsing'
		}
	},
	'ObjectTypeTargetForBrowsing': {
		properties: {
			'TargetObjectType': 'System.Int32',
			'ViewCollection': 'System.Int32'
		},
		methods: {
			'Clone': 'MFilesAPI.ObjectTypeTargetForBrowsing'
		}
	},
	'ObjectTypeAdmin': {
		properties: {
			'ObjectType': 'MFilesAPI.ObjType',
			'DefaultAccessControlList': 'MFilesAPI.AccessControlList',
			'ConnectionString': 'System.String',
			'SelectStatement': 'System.String',
			'SelectStatementOneRecord': 'System.String',
			'InsertIntoStatement': 'System.String',
			'SelectExtIDStatement': 'System.String',
			'UpdateStatement': 'System.String',
			'DeleteStatement': 'System.String',
			'ColumnMappings': 'MFilesAPI.ObjTypeColumnMappings',
			'Translatable': 'System.Boolean',
			'DefaultForAutomaticPermissions': 'MFilesAPI.AutomaticPermissions',
			'SemanticAliases': 'MFilesAPI.SemanticAliases'
		},
		methods: {
			'Clone': 'MFilesAPI.ObjTypeAdmin'
		}
	},
	'ObjectTypeColumnMappings': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjTypeColumnMapping'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.ObjTypeColumnMappings'
		}
	},
	'ObjectTypeColumnMapping': {
		properties: {
			'ObjectType': 'System.Int32',
			'Ordinal': 'System.Int32',
			'TargetPropertyDef': 'System.Int32',
			'SourceField': 'System.String',
			'Type': 'System.Int16',
			'PartOfInsert': 'System.Boolean',
			'PartOfUpdate': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.ObjTypeColumnMapping'
		}
	},
	'ObjectTypesAdmin': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjTypeAdmin'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'ObjectTypes': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjType'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'ObjectVersion': {
		properties: {
			'Title': 'System.String',
			'SingleFile': 'System.Boolean',
			'CreatedUtc': 'System.DateTime',
			'LastModifiedUtc': 'System.DateTime',
			'FilesCount': 'System.Int32',
			'Files': 'MFilesAPI.ObjectFiles',
			'Deleted': 'System.Boolean',
			'CheckedOutVersion': 'System.Int32',
			'CheckedOutTo': 'System.Int32',
			'CheckedOutToUserName': 'System.String',
			'CheckedOutToHostName': 'System.String',
			'CheckedOutAtUtc': 'System.DateTime',
			'LatestCheckedInVersion': 'System.Int32',
			'HasRelationshipsFromThis': 'System.Boolean',
			'HasRelationshipsToThis': 'System.Boolean',
			'ObjVer': 'MFilesAPI.ObjVer',
			'DisplayID': 'System.String',
			'DisplayIDAvailable': 'System.Boolean',
			'ObjectCheckedOut': 'System.Boolean',
			'ObjectCheckedOutToThisUser': 'System.Boolean',
			'ThisVersionCheckedOut': 'System.Boolean',
			'ThisVersionLatestToThisUser': 'System.Boolean',
			'LatestCheckedInVersionOrCheckedOutVersion': 'System.Boolean',
			'VisibleAfterOperation': 'System.Boolean',
			'HasAssignments': 'System.Boolean',
			'ObjectVersionFlags': 'MFilesAPI.MFObjectVersionFlag',
			'Class': 'System.Int32',
			'IsStub': 'System.Boolean',
			'ObjectGUID': 'System.String',
			'IsAccessedByMeValid': 'System.Boolean',
			'AccessedByMeUtc': 'System.DateTime',
			'VersionGUID': 'System.String',
			'OriginalVaultGUID': 'System.String',
			'OriginalObjID': 'MFilesAPI.ObjID',
			'ObjectFlags': 'MFilesAPI.MFSpecialObjectFlag',
			'IsObjectShortcut': 'System.Boolean',
			'IsObjectConflict': 'System.Boolean',
			'HasSharedFiles': 'System.Boolean'
		},
		methods: {
			'GetNameForFileSystemEx': 'System.String',
			'Clone': 'MFilesAPI.ObjectVersion',
			'GetNameForFileSystem': 'System.String'
		}
	},
	'ObjectFiles': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjectFile'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Sort': 'System.Void',
			'GetObjectFileByNameForFileSystem': 'MFilesAPI.ObjectFile',
			'GetObjectFileIndexByNameForFileSystem': 'System.Int32'
		}
	},
	'ObjectFileComparer': {
		properties: {

		},
		methods: {
			'Compare': 'System.Int32'
		}
	},
	'ObjectVersionAndPropertiesOfMultipleObjects': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjectVersionAndProperties'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.ObjectVersionAndPropertiesOfMultipleObjects'
		}
	},
	'ObjectVersionAndProperties': {
		properties: {
			'ObjVer': 'MFilesAPI.ObjVer',
			'VersionData': 'MFilesAPI.ObjectVersion',
			'Properties': 'MFilesAPI.PropertyValues',
			'Vault': 'MFilesAPI.Vault'
		},
		methods: {
			'Clone': 'MFilesAPI.ObjectVersionAndProperties'
		}
	},
	'PropertyValues': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.PropertyValue'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'SearchForProperty': 'MFilesAPI.PropertyValue',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.PropertyValues',
			'IndexOf': 'System.Int32'
		}
	},
	'PropertyValue': {
		properties: {
			'PropertyDef': 'System.Int32',
			'TypedValue': 'MFilesAPI.TypedValue',
			'Value': 'MFilesAPI.TypedValue'
		},
		methods: {
			'Clone': 'MFilesAPI.PropertyValue',
			'GetValueAsText': 'System.String',
			'GetValueAsLocalizedText': 'System.String',
			'GetValueAsUnlocalizedText': 'System.String'
		}
	},
	'TypedValue': {
		properties: {
			'DataType': 'MFilesAPI.MFDataType',
			'DisplayValue': 'System.String',
			'Value': 'System.Object'
		},
		methods: {
			'SetValue': 'System.Void',
			'IsNULL': 'System.Boolean',
			'Clone': 'MFilesAPI.TypedValue',
			'Serialize': 'System.Byte[]',
			'Unserialize': 'System.Void',
			'CompareTo': 'System.Int32',
			'GetValueAsLocalizedText': 'System.String',
			'GetValueAsUnlocalizedText': 'System.String',
			'GetValueAsText': 'System.String',
			'GetLookupID': 'System.Int32',
			'SetValueToNULL': 'System.Void',
			'GetValueAsTextWithExpression': 'System.String',
			'IsEmpty': 'System.Boolean',
			'GetValueAsLookup': 'MFilesAPI.Lookup',
			'SetValueToLookup': 'System.Void',
			'GetValueAsLookups': 'MFilesAPI.Lookups',
			'SetValueToMultiSelectLookup': 'System.Void',
			'IsUninitialized': 'System.Boolean',
			'GetValueAsTimestamp': 'MFilesAPI.Timestamp'
		}
	},
	'Expression': {
		properties: {
			'Type': 'MFilesAPI.MFExpressionType',
			'DataPropertyValuePropertyDef': 'System.Int32',
			'DataPropertyValueDataFunction': 'MFilesAPI.MFDataFunction',
			'DataPropertyValueParentChildBehaviour': 'MFilesAPI.MFParentChildBehavior',
			'DataObjectIDSegmentSegmentSize': 'System.Int32',
			'DataStatusValueType': 'MFilesAPI.MFStatusType',
			'DataStatusValueDataFunction': 'MFilesAPI.MFDataFunction',
			'DataFileValueType': 'MFilesAPI.MFFileValueType',
			'DataTypedValueDatatype': 'MFilesAPI.MFDataType',
			'DataTypedValueValueList': 'System.Int32',
			'DataTypedValueDataFunction': 'MFilesAPI.MFDataFunction',
			'DataTypedValueParentChildBehaviour': 'MFilesAPI.MFParentChildBehavior',
			'DataAnyFieldFTSFlags': 'MFilesAPI.MFFullTextSearchFlags',
			'DataPermissionsType': 'MFilesAPI.MFPermissionsExpressionType',
			'IndirectionLevels': 'MFilesAPI.PropertyDefOrObjectTypes'
		},
		methods: {
			'SetPropertyValueExpression': 'System.Void',
			'SetTypedValueExpression': 'System.Void',
			'SetStatusValueExpression': 'System.Void',
			'SetFileValueExpression': 'System.Void',
			'SetObjectIDSegmentExpression': 'System.Void',
			'SetAnyFieldExpression': 'System.Void',
			'SetPermissionExpression': 'System.Void',
			'SetValueListItemExpression': 'System.Void',
			'GetExpressionAsText': 'System.String',
			'GetWholeExpressionAsIndirectionLevels': 'MFilesAPI.PropertyDefOrObjectTypes',
			'Clone': 'MFilesAPI.Expression'
		}
	},
	'DataFunctionCall': {
		properties: {
			'DataFunction': 'MFilesAPI.MFDataFunction'
		},
		methods: {
			'SetDataNoOp': 'System.Void',
			'SetDataYear': 'System.Void',
			'SetDataMonth': 'System.Void',
			'SetDataYearAndMonth': 'System.Void',
			'SetDataDate': 'System.Void',
			'SetDataDaysFrom': 'System.Void',
			'SetDataDaysTo': 'System.Void',
			'SetDataIntegerSegment': 'System.Void',
			'SetDataLeftChars': 'System.Void',
			'SetDataInitialCharGroup': 'System.Void'
		}
	},
	'PropertyDefOrObjectTypes': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.PropertyDefOrObjectType'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.PropertyDefOrObjectTypes'
		}
	},
	'PropertyDefOrObjectType': {
		properties: {
			'PropertyDef': 'System.Boolean',
			'ID': 'System.Int32'
		},
		methods: {
			'Clone': 'MFilesAPI.PropertyDefOrObjectType',
			'GetAsExpression': 'MFilesAPI.Expression'
		}
	},
	'PropertyDefs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.PropertyDef'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'PropertyDef': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String',
			'DataType': 'MFilesAPI.MFDataType',
			'ContentType': 'MFilesAPI.MFContentType',
			'BasedOnValueList': 'System.Boolean',
			'ValueList': 'System.Int32',
			'UpdateType': 'MFilesAPI.MFUpdateType',
			'Predefined': 'System.Boolean',
			'ThisIsOwnerPD': 'System.Boolean',
			'ThisIsDefaultPD': 'System.Boolean',
			'AllObjectTypes': 'System.Boolean',
			'ObjectType': 'System.Int32',
			'DependencyRelation': 'MFilesAPI.MFDependencyRelation',
			'DependencyPD': 'System.Int32',
			'SortAscending': 'System.Boolean',
			'OwnerPropertyDef': 'MFilesAPI.OwnerPropertyDef',
			'AccessControlList': 'MFilesAPI.AccessControlList',
			'AutomaticValueType': 'MFilesAPI.MFAutomaticValueType',
			'AutomaticValue': 'System.String',
			'ValueListSortingType': 'MFilesAPI.MFValueListSortingType',
			'AutomaticValueDefinition': 'MFilesAPI.TypedValue',
			'StaticFilter': 'MFilesAPI.SearchConditions',
			'ThisIsConflictPD': 'System.Boolean'
		},
		methods: {

		}
	},
	'OwnerPropertyDef': {
		properties: {
			'ID': 'System.Int32',
			'DependencyRelation': 'MFilesAPI.MFDependencyRelation',
			'IsRelationFiltering': 'System.Boolean',
			'IndexForAutomaticFilling': 'System.Int32'
		},
		methods: {

		}
	},
	'SearchConditions': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.SearchCondition'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.SearchConditions',
			'AppendFromExportedSearchString': 'System.Void',
			'GetAsExportedSearchString': 'System.String'
		}
	},
	'SearchCondition': {
		properties: {
			'Expression': 'MFilesAPI.Expression',
			'ConditionType': 'MFilesAPI.MFConditionType',
			'TypedValue': 'MFilesAPI.TypedValue'
		},
		methods: {
			'Clone': 'MFilesAPI.SearchCondition',
			'Set': 'System.Void'
		}
	},
	'Lookup': {
		properties: {
			'Item': 'System.Int32',
			'Version': 'System.Int32',
			'Deleted': 'System.Boolean',
			'Hidden': 'System.Boolean',
			'DisplayValue': 'System.String',
			'ObjectType': 'System.Int32',
			'ItemGUID': 'System.String',
			'DisplayID': 'System.String',
			'DisplayIDAvailable': 'System.Boolean',
			'ObjectFlags': 'MFilesAPI.MFSpecialObjectFlag'
		},
		methods: {
			'GetFormattedDisplayValue': 'System.String',
			'Clone': 'MFilesAPI.Lookup'
		}
	},
	'Lookups': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.Lookup'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.Lookups',
			'GetLookupByItem': 'MFilesAPI.Lookup',
			'GetLookupIndexByItem': 'System.Int32'
		}
	},
	'Timestamp': {
		properties: {
			'Year': 'System.UInt32',
			'Month': 'System.UInt32',
			'Day': 'System.UInt32',
			'Hour': 'System.UInt32',
			'Minute': 'System.UInt32',
			'Second': 'System.UInt32',
			'Fraction': 'System.UInt32'
		},
		methods: {
			'Clone': 'MFilesAPI.Timestamp',
			'SetValue': 'System.Void',
			'GetValue': 'System.Object',
			'UtcToLocalTime': 'MFilesAPI.Timestamp',
			'LocalTimeToUtc': 'MFilesAPI.Timestamp'
		}
	},
	'Vault': {
		properties: {
			'ObjectTypeOperations': 'MFilesAPI.VaultObjectTypeOperations',
			'ObjectOperations': 'MFilesAPI.VaultObjectOperations',
			'ObjectPropertyOperations': 'MFilesAPI.VaultObjectPropertyOperations',
			'ObjectFileOperations': 'MFilesAPI.VaultObjectFileOperations',
			'PropertyDefOperations': 'MFilesAPI.VaultPropertyDefOperations',
			'ValueListOperations': 'MFilesAPI.VaultValueListOperations',
			'ValueListItemOperations': 'MFilesAPI.VaultValueListItemOperations',
			'ClassOperations': 'MFilesAPI.VaultClassOperations',
			'ClassGroupOperations': 'MFilesAPI.VaultClassGroupOperations',
			'WorkflowOperations': 'MFilesAPI.VaultWorkflowOperations',
			'ViewOperations': 'MFilesAPI.VaultViewOperations',
			'UserOperations': 'MFilesAPI.VaultUserOperations',
			'UserGroupOperations': 'MFilesAPI.VaultUserGroupOperations',
			'NamedACLOperations': 'MFilesAPI.VaultNamedACLOperations',
			'TraditionalFolderOperations': 'MFilesAPI.VaultTraditionalFolderOperations',
			'ReadOnlyAccess': 'System.Boolean',
			'LoggedIn': 'System.Boolean',
			'SessionInfo': 'MFilesAPI.SessionInfo',
			'ClientOperations': 'MFilesAPI.VaultClientOperations',
			'ObjectSearchOperations': 'MFilesAPI.VaultObjectSearchOperations',
			'CurrentLoggedInUserID': 'System.Int32',
			'ManagementOperations': 'MFilesAPI.VaultManagementOperations',
			'UserSettingOperations': 'MFilesAPI.VaultUserSettingOperations',
			'VaultLanguages': 'MFilesAPI.Languages',
			'NamedValueStorageOperations': 'MFilesAPI.VaultNamedValueStorageOperations',
			'DataSetOperations': 'MFilesAPI.VaultDataSetOperations',
			'EventLogOperations': 'MFilesAPI.VaultEventLogOperations',
			'Name': 'System.String',
			'ElectronicSignatureOperations': 'MFilesAPI.VaultElectronicSignatureOperations',
			'ScheduledJobManagementOperations': 'MFilesAPI.VaultScheduledJobManagementOperations',
			'CustomApplicationManagementOperations': 'MFilesAPI.VaultCustomApplicationManagementOperations'
		},
		methods: {
			'GetMFilesURLForVaultRoot': 'System.String',
			'TestConnectionToVaultWithTimeout': 'System.Void',
			'LogOutWithDialogs': 'System.Boolean',
			'GetGUID': 'System.String',
			'LogOutSilent': 'System.Void',
			'ChangePassword': 'System.Void',
			'GetServerVersionOfVault': 'MFilesAPI.MFilesVersion',
			'TestConnectionToServer': 'System.Void',
			'TestConnectionToVault': 'System.Void'
		}
	},
	'VaultObjectTypeOperations': {
		properties: {

		},
		methods: {
			'GetBuiltInObjectType': 'MFilesAPI.ObjType',
			'GetObjectTypes': 'MFilesAPI.ObjTypes',
			'GetObjectType': 'MFilesAPI.ObjType',
			'RefreshExternalObjectType': 'System.Void',
			'AddObjectTypeAdmin': 'MFilesAPI.ObjTypeAdmin',
			'RemoveObjectTypeAdmin': 'System.Void',
			'UpdateObjectTypeAdmin': 'MFilesAPI.ObjTypeAdmin',
			'GetObjectTypesAdmin': 'MFilesAPI.ObjTypesAdmin',
			'GetObjectTypeAdmin': 'MFilesAPI.ObjTypeAdmin',
			'UpdateObjectTypeWithAutomaticPermissionsAdmin': 'MFilesAPI.ObjTypeAdmin',
			'GetObjectTypeIDByAlias': 'System.Int32'
		}
	},
	'VaultObjectOperations': {
		properties: {

		},
		methods: {
			'GetObjectGUID': 'System.String',
			'SetObjectGUID': 'System.Void',
			'GetObjIDByGUID': 'MFilesAPI.ObjID',
			'DestroyObjects': 'System.Void',
			'CheckOutMultipleObjects': 'MFilesAPI.ObjectVersions',
			'CheckInMultipleObjects': 'MFilesAPI.ObjectVersions',
			'GetObjIDByOriginalObjID': 'MFilesAPI.ObjID',
			'ResolveConflict': 'MFilesAPI.ObjectVersions',
			'CreateNewObjectExQuick': 'System.Int32',
			'GetMFilesURLForObject': 'System.String',
			'ShowPrefilledNewObjectWindow': 'MFilesAPI.ObjectWindowResult',
			'CreateNewEmptySingleFileDocument': 'MFilesAPI.ObjectVersionAndProperties',
			'SetOfflineAvailability': 'System.Void',
			'GetObjectLocationsInView': 'MFilesAPI.Strings',
			'ShowRelatedObjects': 'System.Void',
			'ShowRelationshipsDialog': 'System.Void',
			'ShowCollectionMembersDialog': 'System.Void',
			'ShowSubObjectsDialogModal': 'System.Void',
			'ShowHistoryDialogModal': 'System.Void',
			'ShowSelectObjectHistoryDialogModal': 'MFilesAPI.ObjOrFileVer',
			'ShowCommentsDialogModal': 'System.Void',
			'ShowCheckInReminder': 'MFilesAPI.ObjectVersion',
			'GetMFilesURLForObjectOrFile': 'System.String',
			'RejectCheckInReminder': 'System.Void',
			'DelayedCheckIn': 'System.Void',
			'ShowCheckInReminderDialogModal': 'System.Boolean',
			'ProposeCheckOutForFile': 'System.Boolean',
			'ForceUndoCheckout': 'MFilesAPI.ObjectVersion',
			'CreateNewSFDObject': 'MFilesAPI.ObjectVersionAndProperties',
			'CreateNewSFDObjectQuick': 'System.Int32',
			'AddFavorites': 'MFilesAPI.ObjectVersionAndPropertiesOfMultipleObjects',
			'AddFavorite': 'MFilesAPI.ObjectVersionAndProperties',
			'RemoveFavorites': 'MFilesAPI.ObjectVersionAndPropertiesOfMultipleObjects',
			'RemoveFavorite': 'MFilesAPI.ObjectVersionAndProperties',
			'NotifyObjectAccess': 'MFilesAPI.ObjectVersionAndProperties',
			'CreateNewAssignment': 'MFilesAPI.ObjectVersionAndProperties',
			'ShowBasicNewObjectWindow': 'MFilesAPI.ObjectWindowResult',
			'ShowNewObjectWindow': 'MFilesAPI.ObjectWindowResult',
			'ShowBasicEditObjectWindow': 'MFilesAPI.ObjectWindowResult',
			'ShowEditObjectWindow': 'MFilesAPI.ObjectWindowResult',
			'CreateNewObject': 'MFilesAPI.ObjectVersionAndProperties',
			'CheckIn': 'MFilesAPI.ObjectVersion',
			'CheckOut': 'MFilesAPI.ObjectVersion',
			'GetObjectVersionAndProperties': 'MFilesAPI.ObjectVersionAndProperties',
			'GetLatestObjectVersionAndProperties': 'MFilesAPI.ObjectVersionAndProperties',
			'IsCheckedOut': 'System.Boolean',
			'ShowCheckoutPrompt': 'MFilesAPI.ObjectVersion',
			'IsSingleFileObject': 'System.Boolean',
			'SetSingleFileObject': 'System.Void',
			'ChangePermissionsToNamedACL': 'MFilesAPI.ObjectVersion',
			'ChangePermissionsToACL': 'MFilesAPI.ObjectVersion',
			'DestroyObject': 'System.Void',
			'GetThumbnailAsBytes': 'System.Byte[]',
			'Rollback': 'MFilesAPI.ObjectVersion',
			'UndeleteObject': 'MFilesAPI.ObjectVersion',
			'UndoCheckout': 'MFilesAPI.ObjectVersion',
			'GetHistory': 'MFilesAPI.ObjectVersions',
			'GetRelationships': 'MFilesAPI.ObjectVersions',
			'GetCollectionMembers': 'MFilesAPI.ObjectVersions',
			'GetObjectInfo': 'MFilesAPI.ObjectVersion',
			'CreateNewObjectEx': 'MFilesAPI.ObjectVersionAndProperties',
			'GetObjectPermissions': 'MFilesAPI.ObjectVersionPermissions',
			'RemoveObject': 'MFilesAPI.ObjectVersion',
			'GetLatestObjVer': 'MFilesAPI.ObjVer'
		}
	},
	'ObjectWindowResult': {
		properties: {
			'Result': 'MFilesAPI.MFObjectWindowResultCode',
			'Properties': 'MFilesAPI.PropertyValues',
			'Visible': 'System.Boolean',
			'SelectedFileClass': 'MFilesAPI.FileClass',
			'ObjVer': 'MFilesAPI.ObjVer',
			'AccessControlList': 'MFilesAPI.AccessControlList',
			'UseAsDefaults': 'System.Boolean'
		},
		methods: {

		}
	},
	'FileClass': {
		properties: {
			'FileClass': 'System.String',
			'DotAndExtension': 'System.String',
			'Extension': 'System.String',
			'DisplayName': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.FileClass',
			'LoadByExtension': 'System.Void',
			'SetFileClassInfo': 'System.Void',
			'LoadByFileClass': 'System.Void'
		}
	},
	'ObjectCreationInfo': {
		properties: {
			'SkipThisButtonVisible': 'System.Boolean',
			'OkToAllButtonVisible': 'System.Boolean',
			'ObjectGUID': 'System.String',
			'IsObjectShortcut': 'System.Boolean',
			'ExplicitObjectID': 'System.Int32'
		},
		methods: {
			'SetUseAsDefaults': 'System.Void',
			'GetUseAsDefaults': 'System.Boolean',
			'SetTitle': 'System.Void',
			'GetTitle': 'MFilesAPI.TypedValue',
			'SetExtension': 'System.Void',
			'GetExtension': 'System.String',
			'SetObjectType': 'System.Void',
			'GetObjectType': 'System.Int32',
			'SetDisableObjectCreation': 'System.Void',
			'GetDisableObjectCreation': 'System.Boolean',
			'SetSingleFileDocument': 'System.Void',
			'GetSingleFileDocument': 'System.Boolean',
			'SetSelectedFileClass': 'System.Void',
			'GetSelectedFileClass': 'MFilesAPI.FileClass',
			'SetSelectableFileClasses': 'System.Void',
			'GetSelectableFileClasses': 'MFilesAPI.FileClasses',
			'SetSourceFiles': 'System.Void',
			'GetSourceFiles': 'MFilesAPI.SourceObjectFiles',
			'SetMetadataCardTitle': 'System.Void',
			'GetMetadataCardTitle': 'System.String',
			'SetHideGeneralFrame': 'System.Void',
			'GetHideGeneralFrame': 'System.Boolean',
			'SetTemplate': 'System.Void',
			'GetTemplate': 'MFilesAPI.ObjVer',
			'SetTitleAsDatatypeText': 'System.Void',
			'GetTitleAsText': 'System.String'
		}
	},
	'FileClasses': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.FileClass'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.FileClasses'
		}
	},
	'SourceObjectFiles': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.SourceObjectFile'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'AddFile': 'MFilesAPI.SourceObjectFile',
			'AddEmptyFile': 'MFilesAPI.SourceObjectFile'
		}
	},
	'SourceObjectFile': {
		properties: {
			'Title': 'System.String',
			'Extension': 'System.String',
			'SourceFilePath': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.SourceObjectFile'
		}
	},
	'ObjectVersions': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjectVersion'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Sort': 'System.Void',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'GetAsObjVers': 'MFilesAPI.ObjVers'
		}
	},
	'ObjectComparer': {
		properties: {

		},
		methods: {
			'Compare': 'System.Int32'
		}
	},
	'ObjVers': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjVer'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.ObjVers',
			'GetAllDistinctObjIDs': 'MFilesAPI.ObjIDs'
		}
	},
	'ObjectVersionPermissions': {
		properties: {
			'CustomACL': 'System.Boolean',
			'NamedACL': 'MFilesAPI.NamedACL',
			'AccessControlList': 'MFilesAPI.AccessControlList',
			'ObjVer': 'MFilesAPI.ObjVer'
		},
		methods: {

		}
	},
	'Strings': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'System.String'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void'
		}
	},
	'ObjectFileAndVersion': {
		properties: {
			'ObjectFile': 'MFilesAPI.ObjectFile',
			'ObjectVersion': 'MFilesAPI.ObjectVersionAndProperties'
		},
		methods: {

		}
	},
	'VaultObjectPropertyOperations': {
		properties: {

		},
		methods: {
			'GetProperties': 'MFilesAPI.PropertyValues',
			'SetProperties': 'MFilesAPI.ObjectVersionAndProperties',
			'GetProperty': 'MFilesAPI.PropertyValue',
			'SetProperty': 'MFilesAPI.ObjectVersionAndProperties',
			'RemoveProperty': 'MFilesAPI.ObjectVersionAndProperties',
			'GetPropertiesForDisplay': 'MFilesAPI.PropertyValuesForDisplay',
			'GetAssignment_DEPRECATED': 'MFilesAPI.WorkflowAssignment',
			'SetAssignment_DEPRECATED': 'MFilesAPI.ObjectVersionAndProperties',
			'MarkAssignmentComplete': 'MFilesAPI.ObjectVersionAndProperties',
			'SetAllProperties': 'MFilesAPI.ObjectVersionAndProperties',
			'SetVersionComment': 'MFilesAPI.ObjectVersionAndProperties',
			'GetVersionComment': 'MFilesAPI.VersionComment',
			'GetVersionCommentHistory': 'MFilesAPI.VersionComments',
			'SetWorkflowState': 'MFilesAPI.ObjectVersionAndProperties',
			'GetWorkflowState': 'MFilesAPI.ObjectVersionWorkflowState',
			'GetPropertiesAsXML': 'System.String',
			'GetPropertiesOfMultipleObjects': 'MFilesAPI.PropertyValuesOfMultipleObjects',
			'SetCreationInfoAdmin': 'MFilesAPI.ObjectVersionAndProperties',
			'SetLastModificationInfoAdmin': 'MFilesAPI.ObjectVersionAndProperties',
			'SetPropertiesOfMultipleObjects': 'MFilesAPI.ObjectVersionAndPropertiesOfMultipleObjects',
			'GetPropertiesWithIconClues': 'MFilesAPI.PropertyValuesWithIconClues',
			'GetPropertiesWithIconCluesOfMultipleObjects': 'MFilesAPI.PropertyValuesWithIconCluesOfMultipleObjects',
			'SetPropertiesWithPermissions': 'MFilesAPI.ObjectVersionAndProperties',
			'SetAllPropertiesWithPermissions': 'MFilesAPI.ObjectVersionAndProperties',
			'GenerateAutomaticPermissionsFromPropertyValues': 'MFilesAPI.AccessControlList',
			'GetPropertiesForMetadataSync': 'MFilesAPI.NamedValues'
		}
	},
	'PropertyValuesForDisplay': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.PropertyValueForDisplay'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'PropertyValueForDisplay': {
		properties: {
			'PropertyDef': 'System.Int32',
			'PropertyDefName': 'System.String',
			'ReadOnly': 'System.Boolean',
			'DisplayValue': 'System.String',
			'DataType': 'MFilesAPI.MFDataType',
			'ContentType': 'MFilesAPI.MFContentType',
			'PropertyValue': 'MFilesAPI.PropertyValue'
		},
		methods: {
			'Clone': 'MFilesAPI.PropertyValueForDisplay'
		}
	},
	'WorkflowAssignment': {
		properties: {
			'CompletedBy_DEPRECATED': 'MFilesAPI.PropertyValue',
			'MonitoredBy_DEPRECATED': 'MFilesAPI.PropertyValue',
			'Description_DEPRECATED': 'MFilesAPI.PropertyValue',
			'Deadline_DEPRECATED': 'MFilesAPI.PropertyValue',
			'AssignedTo_DEPRECATED': 'MFilesAPI.PropertyValue'
		},
		methods: {
			'Clone_DEPRECATED': 'MFilesAPI.WorkflowAssignment'
		}
	},
	'VersionComment': {
		properties: {
			'ObjVer': 'MFilesAPI.ObjVer',
			'VersionComment': 'MFilesAPI.PropertyValue',
			'LastModifiedBy': 'MFilesAPI.PropertyValue',
			'StatusChanged': 'MFilesAPI.PropertyValue'
		},
		methods: {

		}
	},
	'VersionComments': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.VersionComment'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'ObjectVersionWorkflowState': {
		properties: {
			'Workflow': 'MFilesAPI.PropertyValue',
			'State': 'MFilesAPI.PropertyValue',
			'VersionComment': 'MFilesAPI.PropertyValue'
		},
		methods: {

		}
	},
	'PropertyValuesOfMultipleObjects': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.PropertyValues'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'SetPropertiesParamsOfMultipleObjects': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.SetPropertiesParams'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.SetPropertiesParamsOfMultipleObjects'
		}
	},
	'SetPropertiesParams': {
		properties: {
			'ObjVer': 'MFilesAPI.ObjVer',
			'AllowModifyingCheckedInObject': 'System.Boolean',
			'FailIfNotLatestCheckedInVersion': 'System.Boolean',
			'PropertyValuesToSet': 'MFilesAPI.PropertyValues',
			'FullSet': 'System.Boolean',
			'PropertyValuesToRemove': 'MFilesAPI.IDs',
			'DisallowNameChange': 'System.Boolean',
			'AccessControlListEnforcingMode': 'MFilesAPI.MFACLEnforcingMode',
			'AccessControlListProvidedForEnforcing': 'MFilesAPI.AccessControlList'
		},
		methods: {
			'Set': 'System.Void',
			'Clone': 'MFilesAPI.SetPropertiesParams',
			'SetWithPermissions': 'System.Void'
		}
	},
	'PropertyValuesWithIconClues': {
		properties: {
			'PropertyValues': 'MFilesAPI.PropertyValues',
			'IconClues': 'MFilesAPI.PropertyValueIconClues',
			'ObjVer': 'MFilesAPI.ObjVer'
		},
		methods: {

		}
	},
	'PropertyValueIconClues': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.PropertyValueIconClue'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'PropertyValueIconClue': {
		properties: {
			'PropertyDef': 'System.Int32',
			'ValueListItem': 'System.Int32'
		},
		methods: {

		}
	},
	'PropertyValuesWithIconCluesOfMultipleObjects': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.PropertyValuesWithIconClues'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'NamedValues': {
		properties: {
			'Value': 'System.Object',
			'Modified': 'System.Boolean',
			'Names': 'MFilesAPI.Strings'
		},
		methods: {
			'Clone': 'MFilesAPI.NamedValues',
			'Contains': 'System.Boolean'
		}
	},
	'VaultObjectFileOperations': {
		properties: {

		},
		methods: {
			'AddFile': 'MFilesAPI.FileVer',
			'RemoveFile': 'MFilesAPI.ObjectVersion',
			'GetFiles': 'MFilesAPI.ObjectFiles',
			'GetPathInDefaultView': 'System.String',
			'DownloadFile': 'System.Void',
			'DownloadFileInBlocks_Begin': 'MFilesAPI.FileDownloadSession',
			'DownloadFileInBlocks_ReadBlock': 'System.Byte[]',
			'DownloadFileInBlocks_Begin_32bit': 'MFilesAPI.FileDownloadSession',
			'DownloadFileInBlocks_ReadBlock_32bit': 'System.Byte[]',
			'UploadFile': 'System.Void',
			'UploadFileBlockBegin': 'System.Int32',
			'UploadFileBlock': 'System.Void',
			'UploadFileBlockBegin_32bit': 'System.Int32',
			'UploadFileBlock_32bit': 'System.Void',
			'UploadFileCommit': 'System.Void',
			'UploadFileCommit_32bit': 'System.Void',
			'RenameFile': 'MFilesAPI.ObjectVersion',
			'GetFileSize': 'System.Int64',
			'GetLatestFileVersion': 'MFilesAPI.FileVer',
			'AddEmptyFile': 'MFilesAPI.FileVer',
			'OpenFileInDefaultApplication': 'System.Void',
			'GetFileSize_32bit': 'System.Int32',
			'GetObjIDOfFile': 'MFilesAPI.ObjID',
			'PerformOCROperation': 'MFilesAPI.OCRPageResults',
			'GetFilesForModificationInEventHandler': 'MFilesAPI.ObjectFiles',
			'ConvertToPDF': 'MFilesAPI.ObjectVersion'
		}
	},
	'FileDownloadSession': {
		properties: {
			'DownloadID': 'System.Int32',
			'FileSize': 'System.Int64',
			'FileSize32': 'System.Int32'
		},
		methods: {

		}
	},
	'OCROptions': {
		properties: {
			'PrimaryLanguage': 'MFilesAPI.MFOCRLanguage',
			'SecondaryLanguage': 'MFilesAPI.MFOCRLanguage'
		},
		methods: {
			'Clone': 'MFilesAPI.OCROptions'
		}
	},
	'OCRPages': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.OCRPage'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.OCRPages'
		}
	},
	'OCRPage': {
		properties: {
			'PageNum': 'System.Int32',
			'OCRZones': 'MFilesAPI.OCRZones'
		},
		methods: {
			'Clone': 'MFilesAPI.OCRPage'
		}
	},
	'OCRZones': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.OCRZone'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.OCRZones'
		}
	},
	'OCRZone': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String',
			'DimensionUnit': 'MFilesAPI.MFOCRDimensionUnit',
			'Left': 'System.Int32',
			'Top': 'System.Int32',
			'Width': 'System.Int32',
			'Height': 'System.Int32',
			'OCROptions': 'MFilesAPI.OCROptions',
			'HasOCROptions': 'System.Boolean',
			'DataType': 'MFilesAPI.MFDataType',
			'Barcode': 'System.Boolean'
		},
		methods: {
			'SetOCROptions': 'System.Void',
			'ClearOCROptions': 'System.Void',
			'Clone': 'MFilesAPI.OCRZone'
		}
	},
	'OCRPageResults': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.OCRPageResult'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'OCRPageResult': {
		properties: {
			'PageNum': 'System.Int32',
			'OCRZoneResults': 'MFilesAPI.OCRZoneResults'
		},
		methods: {

		}
	},
	'OCRZoneResults': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.OCRZoneResult'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'OCRZoneResult': {
		properties: {
			'ID': 'System.Int32',
			'DimensionUnit': 'MFilesAPI.MFOCRDimensionUnit',
			'Left': 'System.Int32',
			'Top': 'System.Int32',
			'Width': 'System.Int32',
			'Height': 'System.Int32',
			'Confidence': 'System.Int32',
			'RecognizedOnPage': 'System.Int32',
			'ResultValue': 'MFilesAPI.TypedValue'
		},
		methods: {

		}
	},
	'VaultPropertyDefOperations': {
		properties: {

		},
		methods: {
			'GetPropertyDefs': 'MFilesAPI.PropertyDefs',
			'GetPropertyDef': 'MFilesAPI.PropertyDef',
			'GetBuiltInPropertyDef': 'MFilesAPI.PropertyDef',
			'AddPropertyDefAdmin': 'MFilesAPI.PropertyDefAdmin',
			'RemovePropertyDefAdmin': 'System.Void',
			'UpdatePropertyDefAdmin': 'System.Void',
			'GetPropertyDefsAdmin': 'MFilesAPI.PropertyDefsAdmin',
			'GetPropertyDefAdmin': 'MFilesAPI.PropertyDefAdmin',
			'UpdatePropertyDefWithAutomaticPermissionsAdmin': 'System.Void',
			'GetPropertyDefIDByAlias': 'System.Int32'
		}
	},
	'PropertyDefAdmin': {
		properties: {
			'PropertyDef': 'MFilesAPI.PropertyDef',
			'AutomaticValue': 'MFilesAPI.AutomaticValue',
			'Validation': 'MFilesAPI.Validation',
			'AllowAutomaticPermissions': 'System.Boolean',
			'SemanticAliases': 'MFilesAPI.SemanticAliases'
		},
		methods: {
			'Clone': 'MFilesAPI.PropertyDefAdmin'
		}
	},
	'AutomaticValue': {
		properties: {
			'ANSIncrement': 'System.Int32',
			'ANVCode': 'System.String',
			'CVSExpression': 'System.String',
			'CVVCode': 'System.String',
			'CalculationOrderNumber': 'System.Int32'
		},
		methods: {
			'Clone': 'MFilesAPI.AutomaticValue'
		}
	},
	'Validation': {
		properties: {
			'RegularExpression': 'System.String',
			'VBScript': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.Validation'
		}
	},
	'PropertyDefsAdmin': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.PropertyDefAdmin'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'VaultValueListOperations': {
		properties: {

		},
		methods: {
			'GetValueLists': 'MFilesAPI.ObjTypes',
			'GetValueList': 'MFilesAPI.ObjType',
			'GetBuiltInValueList': 'MFilesAPI.ObjType',
			'RefreshExternalValueList': 'System.Void'
		}
	},
	'VaultValueListItemOperations': {
		properties: {

		},
		methods: {
			'GetValueListItems': 'MFilesAPI.ValueListItems',
			'AddValueListItem': 'MFilesAPI.ValueListItem',
			'UpdateValueListItem': 'System.Void',
			'RemoveValueListItem': 'System.Void',
			'SearchForValueListItems': 'MFilesAPI.ValueListItemSearchResults',
			'GetValueListItemByID': 'MFilesAPI.ValueListItem',
			'ChangePermissionsToNamedACL': 'System.Void',
			'ChangePermissionsToACL': 'System.Void',
			'ChangeDefaultPermissionsToNamedACL': 'System.Void',
			'ChangeDefaultPermissionsToACL': 'System.Void',
			'GetValueListItemByDisplayID': 'MFilesAPI.ValueListItem',
			'GetValueListItemsEx': 'MFilesAPI.ValueListItems',
			'SearchForValueListItemsEx': 'MFilesAPI.ValueListItemSearchResults',
			'GetValueListItemByIDEx': 'MFilesAPI.ValueListItem',
			'GetValueListItemByDisplayIDEx': 'MFilesAPI.ValueListItem',
			'GetValueListItemsEx2': 'MFilesAPI.ValueListItems',
			'SearchForValueListItemsEx2': 'MFilesAPI.ValueListItemSearchResults',
			'GetValueListItemsWithPermissions': 'MFilesAPI.ValueListItemsWithPermissions',
			'SearchForValueListItemsWithPermissions': 'MFilesAPI.ValueListItemSearchResultsWithPermissions',
			'ChangeAutomaticPermissionsToNamedACL': 'System.Void',
			'ChangeAutomaticPermissionsToACL': 'System.Void',
			'ChangeAutomaticPermissionsToItemsOwnPermissions': 'System.Void',
			'ClearAutomaticPermissions': 'System.Void'
		}
	},
	'ValueListItems': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ValueListItem'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'ValueListItem': {
		properties: {
			'ID': 'System.Int32',
			'ValueListID': 'System.Int32',
			'Name': 'System.String',
			'HasParent': 'System.Boolean',
			'ParentID': 'System.Int32',
			'HasOwner': 'System.Boolean',
			'OwnerID': 'System.Int32',
			'DisplayID': 'System.String',
			'DisplayIDAvailable': 'System.Boolean',
			'ACLForObjects': 'MFilesAPI.AccessControlList',
			'Icon': 'System.Byte[]',
			'AutomaticPermissionsForObjects': 'MFilesAPI.AutomaticPermissions',
			'ItemGUID': 'System.String'
		},
		methods: {
			'GetIconAsPNG': 'System.Byte[]',
			'Clone': 'MFilesAPI.ValueListItem'
		}
	},
	'ValueListItemSearchResults': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ValueListItem',
			'MoreResults': 'System.Boolean'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'ValueListItemsWithPermissions': {
		properties: {
			'ValueListItems': 'MFilesAPI.ValueListItems',
			'Permissions': 'MFilesAPI.AccessControlLists'
		},
		methods: {

		}
	},
	'AccessControlLists': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.AccessControlList'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'ValueListItemSearchResultsWithPermissions': {
		properties: {
			'ValueListItemSearchResults': 'MFilesAPI.ValueListItemSearchResults',
			'Permissions': 'MFilesAPI.AccessControlLists'
		},
		methods: {

		}
	},
	'VaultClassOperations': {
		properties: {

		},
		methods: {
			'GetObjectClasses': 'MFilesAPI.ObjectClasses',
			'GetObjectClass': 'MFilesAPI.ObjectClass',
			'GetAllObjectClasses': 'MFilesAPI.ObjectClasses',
			'AddObjectClassAdmin': 'MFilesAPI.ObjectClassAdmin',
			'RemoveObjectClassAdmin': 'System.Void',
			'UpdateObjectClassAdmin': 'System.Void',
			'GetAllObjectClassesAdmin': 'MFilesAPI.ObjectClassesAdmin',
			'GetObjectClassesAdmin': 'MFilesAPI.ObjectClassesAdmin',
			'GetObjectClassAdmin': 'MFilesAPI.ObjectClassAdmin',
			'GetObjectClassIDByAlias': 'System.Int32'
		}
	},
	'VaultClassGroupOperations': {
		properties: {

		},
		methods: {
			'GetClassGroups': 'MFilesAPI.ClassGroups',
			'AddClassGroup': 'MFilesAPI.ClassGroup',
			'RemoveClassGroup': 'System.Void',
			'UpdateClassGroup': 'System.Void',
			'GetClassGroup': 'MFilesAPI.ClassGroup'
		}
	},
	'ClassGroups': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ClassGroup'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'ClassGroup': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String',
			'Members': 'MFilesAPI.IDs',
			'ObjectType': 'System.Int32'
		},
		methods: {
			'Clone': 'MFilesAPI.ClassGroup',
			'AddMember': 'System.Void',
			'RemoveMember': 'System.Void'
		}
	},
	'VaultWorkflowOperations': {
		properties: {

		},
		methods: {
			'GetWorkflowsAsValueListItems': 'MFilesAPI.ValueListItems',
			'GetWorkflowStates': 'MFilesAPI.States',
			'GetWorkflowsForClient': 'MFilesAPI.Workflows',
			'AddWorkflowAdmin': 'MFilesAPI.WorkflowAdmin',
			'RemoveWorkflowAdmin': 'System.Void',
			'UpdateWorkflowAdmin': 'MFilesAPI.WorkflowAdmin',
			'GetWorkflowsAdmin': 'MFilesAPI.WorkflowsAdmin',
			'GetWorkflowForClient': 'MFilesAPI.Workflow',
			'GetWorkflowAdmin': 'MFilesAPI.WorkflowAdmin',
			'GetWorkflowStatesEx': 'MFilesAPI.States',
			'GetWorkflowIDByAlias': 'System.Int32',
			'GetWorkflowStateIDByAlias': 'System.Int32'
		}
	},
	'States': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.State'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'State': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String',
			'Selectable': 'System.Boolean',
			'SelectableFlagAffectedByPseudoUsers': 'System.Boolean'
		},
		methods: {

		}
	},
	'Workflows': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.Workflow'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'Workflow': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String',
			'ObjectClass': 'System.Int32'
		},
		methods: {

		}
	},
	'WorkflowAdmin': {
		properties: {
			'Workflow': 'MFilesAPI.Workflow',
			'States': 'MFilesAPI.StatesAdmin',
			'StateTransitions': 'MFilesAPI.StateTransitions',
			'Permissions': 'MFilesAPI.AccessControlList',
			'Description': 'System.String',
			'SemanticAliases': 'MFilesAPI.SemanticAliases'
		},
		methods: {
			'Clone': 'MFilesAPI.WorkflowAdmin'
		}
	},
	'StatesAdmin': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.StateAdmin'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.StatesAdmin'
		}
	},
	'StateAdmin': {
		properties: {
			'ActionSetPermissions': 'System.Boolean',
			'ActionDelete': 'System.Boolean',
			'ActionMarkForArchiving': 'System.Boolean',
			'ActionAssignToUser': 'System.Boolean',
			'ActionSendNotification': 'System.Boolean',
			'ActionSetProperties': 'System.Boolean',
			'ActionRunVBScript': 'System.Boolean',
			'ID': 'System.Int32',
			'AutomaticStateTransitionNextState': 'System.Int32',
			'Name': 'System.String',
			'Description': 'System.String',
			'ActionRunVBScriptDefinition': 'System.String',
			'AutomaticStateTransitionAllowedByVBScript': 'System.String',
			'RestrictTransitions': 'System.Boolean',
			'AutomaticStateTransitionInDays': 'System.Int32',
			'AutomaticStateTransitionMode': 'MFilesAPI.MFAutoStateTransitionMode',
			'StateACL': 'MFilesAPI.AccessControlList',
			'ActionSetPermissionsDefinition': 'MFilesAPI.AccessControlList',
			'ActionAssignToUserDefinition': 'MFilesAPI.ActionCreateAssignment',
			'ActionSendNotificationDefinition': 'MFilesAPI.ActionSendNotification',
			'ActionSetPropertiesDefinition': 'MFilesAPI.ActionSetProperties',
			'AutomaticStateTransitionCriteria': 'MFilesAPI.SearchConditions',
			'Preconditions': 'MFilesAPI.StateConditions',
			'Postconditions': 'MFilesAPI.StateConditions',
			'ActionCreateSeparateAssignment': 'System.Boolean',
			'ActionCreateSeparateAssignmentDefinition': 'MFilesAPI.ActionCreateAssignment',
			'ActionSetPermissionsDetailedDefinition': 'MFilesAPI.ActionSetPermissions',
			'SemanticAliases': 'MFilesAPI.SemanticAliases',
			'ActionConvertToPDFDefinition': 'MFilesAPI.ActionConvertToPDF',
			'TransitionsRequireEditAccessToObject': 'System.Boolean',
			'CheckInOutPermissions': 'System.Boolean',
			'InOutPermissions': 'MFilesAPI.AccessControlList',
			'ActionConvertToPDF': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.StateAdmin'
		}
	},
	'ActionCreateAssignment': {
		properties: {
			'AssignedToUsers': 'MFilesAPI.UserOrUserGroupIDs',
			'MonitoredByUsers': 'MFilesAPI.UserOrUserGroupIDs',
			'Description': 'System.String',
			'Deadline': 'System.Boolean',
			'DeadlineInDays': 'System.Int32',
			'Title': 'System.String',
			'AssignedTo': 'MFilesAPI.UserOrUserGroupIDExs',
			'MonitoredBy': 'MFilesAPI.UserOrUserGroupIDExs'
		},
		methods: {
			'Clone': 'MFilesAPI.ActionCreateAssignment'
		}
	},
	'UserOrUserGroupIDExs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.UserOrUserGroupIDEx'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'GetUserOrUserGroupIDEx': 'MFilesAPI.UserOrUserGroupIDEx',
			'GetUserOrUserGroupIDExIndex': 'System.Int32',
			'Clone': 'MFilesAPI.UserOrUserGroupIDExs'
		}
	},
	'UserOrUserGroupIDEx': {
		properties: {
			'UserOrGroupType': 'MFilesAPI.MFUserOrUserGroupType',
			'UserOrGroupID': 'System.Int32',
			'WorkflowState': 'System.Int32',
			'IndirectProperty': 'MFilesAPI.IndirectPropertyID'
		},
		methods: {
			'SetUserAccount': 'System.Void',
			'SetUserGroup': 'System.Void',
			'SetWorkflowStatePseudoUser': 'System.Void',
			'SetIndirectPropertyPseudoUser': 'System.Void',
			'Clone': 'MFilesAPI.UserOrUserGroupIDEx'
		}
	},
	'ActionSendNotification': {
		properties: {
			'Subject': 'System.String',
			'Message': 'System.String',
			'Recipients': 'MFilesAPI.UserOrUserGroupIDs'
		},
		methods: {
			'Clone': 'MFilesAPI.ActionSendNotification'
		}
	},
	'ActionSetProperties': {
		properties: {
			'Properties': 'MFilesAPI.DefaultProperties'
		},
		methods: {
			'Clone': 'MFilesAPI.ActionSetProperties'
		}
	},
	'DefaultProperties': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.DefaultProperty'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.DefaultProperties'
		}
	},
	'DefaultProperty': {
		properties: {
			'PropertyDefID': 'System.Int32',
			'Type': 'MFilesAPI.MFDefaultPropertyType',
			'DataFixedValueValue': 'MFilesAPI.TypedValue',
			'DataFromHPDSSXMLPromptName': 'System.String',
			'DataFromHPDSSXMLTreatLookupAsID': 'System.Boolean',
			'DataFromHPDSSXMLAddVLItemIfNotFound': 'System.Boolean',
			'DataFromXMLXPathExpression': 'System.String',
			'DataFromXMLTreatLookupAsID': 'System.Boolean',
			'DataFromXMLAddVLItemIfNotFound': 'System.Boolean',
			'DataFromEmailField': 'MFilesAPI.MFEmailField',
			'DataFromEmailTreatLookupAsID': 'System.Boolean',
			'DataFromEmailAddVLItemIfNotFound': 'System.Boolean',
			'DataFromEmailHeaderField': 'System.String',
			'DataFromEmailHeaderTreatLookupAsID': 'System.Boolean',
			'DataFromEmailHeaderAddVLItemIfNotFound': 'System.Boolean',
			'DataFromOCRZone': 'MFilesAPI.OCRZone',
			'DataFromOCRTreatLookupAsID': 'System.Boolean',
			'DataFromOCRAddVLItemIfNotFound': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.DefaultProperty',
			'SetFixedValue': 'System.Void',
			'SetFromHPDSSXML': 'System.Void',
			'SetFromXML': 'System.Void',
			'SetFromEmail': 'System.Void',
			'SetFromEmailHeader': 'System.Void',
			'SetFromOCR': 'System.Void'
		}
	},
	'StateConditions': {
		properties: {
			'PropertyConditions': 'System.Boolean',
			'VBScript': 'System.Boolean',
			'PropertyConditionsDefinition': 'MFilesAPI.SearchConditions',
			'VBScriptDefinition': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.StateConditions'
		}
	},
	'ActionSetPermissions': {
		properties: {
			'Permissions': 'MFilesAPI.AccessControlList',
			'DiscardsAutomaticPermissions': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.ActionSetPermissions'
		}
	},
	'ActionConvertToPDF': {
		properties: {
			'PDFA1b': 'System.Boolean',
			'StoreAsSeparateFile': 'System.Boolean',
			'OverwriteExistingFile': 'System.Boolean',
			'FailOnUnsupportedSourceFiles': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.ActionConvertToPDF'
		}
	},
	'StateTransitions': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.StateTransition'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.StateTransitions'
		}
	},
	'StateTransition': {
		properties: {
			'FromState': 'System.Int32',
			'ToState': 'System.Int32',
			'AccessControlList': 'MFilesAPI.AccessControlList',
			'SignatureSettings': 'MFilesAPI.SignatureSettings'
		},
		methods: {
			'Clone': 'MFilesAPI.StateTransition'
		}
	},
	'SignatureSettings': {
		properties: {
			'IsRequired': 'System.Boolean',
			'Meaning': 'System.String',
			'Manifestation': 'System.String',
			'ManifestationPropertyID': 'System.Int32',
			'Reason': 'System.String',
			'AdditionalInfo': 'System.String',
			'IsSeparateSignatureObject': 'System.Boolean',
			'SignatureIdentifier': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.SignatureSettings'
		}
	},
	'WorkflowsAdmin': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.WorkflowAdmin'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.WorkflowsAdmin'
		}
	},
	'VaultViewOperations': {
		properties: {

		},
		methods: {
			'GetViews': 'MFilesAPI.Views',
			'GetView': 'MFilesAPI.View',
			'GetFolderNameListing': 'MFilesAPI.FolderNameListing',
			'GetViewLocationInClient': 'System.String',
			'FindPropertyFolderLocationInView': 'System.String',
			'ShowViewOrPropertyFolder': 'System.Void',
			'AddView': 'MFilesAPI.View',
			'UpdateView': 'MFilesAPI.View',
			'RemoveView': 'System.Void',
			'AddOfflineFilter': 'MFilesAPI.View',
			'GetBuiltInView': 'MFilesAPI.View',
			'AddTemporarySearchView': 'MFilesAPI.View',
			'GetTemporarySearchView': 'MFilesAPI.TemporarySearchView',
			'GetMFilesURLForView': 'System.String',
			'GetFolderContents': 'MFilesAPI.FolderContentItems'
		}
	},
	'Views': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.View'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'View': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String',
			'Common': 'System.Boolean',
			'Visible': 'System.Boolean',
			'SearchFlags': 'MFilesAPI.MFSearchFlags',
			'LookInAllVersions': 'System.Boolean',
			'ReturnLatestVisibleVersion': 'System.Boolean',
			'SearchConditions': 'MFilesAPI.SearchConditions',
			'Levels': 'MFilesAPI.ExpressionExs',
			'ViewType': 'MFilesAPI.MFViewType',
			'HasParent': 'System.Boolean',
			'Parent': 'System.Int32',
			'AccessControlList': 'MFilesAPI.AccessControlList',
			'ViewLocation': 'MFilesAPI.ViewLocation',
			'SearchDef': 'MFilesAPI.SearchDef',
			'ViewFlags': 'MFilesAPI.MFViewFlag'
		},
		methods: {

		}
	},
	'ExpressionExs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ExpressionEx'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.ExpressionExs'
		}
	},
	'ExpressionEx': {
		properties: {
			'Expression': 'MFilesAPI.Expression',
			'ShowEmptyFolders': 'System.Boolean',
			'ShowNULLFolder': 'System.Boolean',
			'Conditions': 'MFilesAPI.SearchConditions',
			'FilterLevel': 'System.Int32',
			'ShowNULLFolderContentsOnThisLevel': 'System.Boolean',
			'ShowMatchingObjectsOnThisLevel': 'System.Boolean',
			'NULLFolderName': 'System.String',
			'ShowContentsAsJITFolders': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.ExpressionEx'
		}
	},
	'ViewLocation': {
		properties: {
			'ParentFolder': 'MFilesAPI.FolderDefs',
			'Overlapping': 'System.Boolean',
			'OverlappedFolder': 'MFilesAPI.TypedValue'
		},
		methods: {
			'Clone': 'MFilesAPI.ViewLocation'
		}
	},
	'FolderDefs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.FolderDef'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.FolderDefs'
		}
	},
	'FolderDef': {
		properties: {
			'FolderDefType': 'MFilesAPI.MFFolderDefType',
			'View': 'System.Int32',
			'PropertyFolder': 'MFilesAPI.TypedValue',
			'TraditionalFolder': 'System.Int32',
			'SearchDef': 'MFilesAPI.SearchDef'
		},
		methods: {
			'SetView': 'System.Void',
			'SetPropertyFolder': 'System.Void',
			'SetTraditionalFolder': 'System.Void',
			'SetSearchDef': 'System.Void',
			'Clone': 'MFilesAPI.FolderDef'
		}
	},
	'SearchDef': {
		properties: {
			'Conditions': 'MFilesAPI.SearchConditions',
			'Levels': 'MFilesAPI.ExpressionExs',
			'SearchFlags': 'MFilesAPI.MFSearchFlags',
			'LookInAllVersions': 'System.Boolean',
			'ReturnLatestVisibleVersion': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.SearchDef',
			'IsIndirectionUsed': 'System.Boolean'
		}
	},
	'FolderNameListing': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.TypedValue',
			'MoreValues': 'System.Boolean'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'SortWithExpression': 'System.Void'
		}
	},
	'TypedValues': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.TypedValue'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void'
		}
	},
	'SearchCriteria': {
		properties: {
			'FirstCondition': 'MFilesAPI.SearchConditionEx',
			'SecondCondition': 'MFilesAPI.SearchConditionEx',
			'ObjectTypeCondition': 'MFilesAPI.SearchConditionEx',
			'FullTextSearchString': 'System.String',
			'SearchFlags': 'MFilesAPI.MFSearchFlags',
			'AdditionalConditions': 'MFilesAPI.SearchConditionExs',
			'FullTextSearchFlags': 'MFilesAPI.MFFullTextSearchFlags',
			'ExpandUI': 'System.Boolean',
			'SearchWithinThisFolder': 'System.Boolean',
			'PredefinedSearchFilter': 'MFilesAPI.MFPredefinedSearchFilterType',
			'SearchRefinement': 'MFilesAPI.SearchConditions',
			'PreviousBaseConditions': 'MFilesAPI.SearchConditions'
		},
		methods: {
			'Clone': 'MFilesAPI.SearchCriteria',
			'GetAsSearchConditions': 'MFilesAPI.SearchConditions'
		}
	},
	'SearchConditionEx': {
		properties: {
			'SearchCondition': 'MFilesAPI.SearchCondition',
			'Enabled': 'System.Boolean',
			'SpecialNULL': 'System.Boolean',
			'Ignored': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.SearchConditionEx',
			'Set': 'System.Void'
		}
	},
	'SearchConditionExs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.SearchConditionEx'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.SearchConditionExs'
		}
	},
	'TemporarySearchView': {
		properties: {
			'View': 'MFilesAPI.View',
			'SearchCriteria': 'MFilesAPI.SearchCriteria',
			'BaseSearchConditions': 'MFilesAPI.SearchConditions'
		},
		methods: {

		}
	},
	'FolderContentItems': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.FolderContentItem',
			'MoreResults': 'System.Boolean'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'FolderContentItem': {
		properties: {
			'FolderContentItemType': 'MFilesAPI.MFFolderContentItemType',
			'View': 'MFilesAPI.View',
			'PropertyFolder': 'MFilesAPI.TypedValue',
			'TraditionalFolder': 'MFilesAPI.Lookup',
			'ObjectVersion': 'MFilesAPI.ObjectVersion'
		},
		methods: {

		}
	},
	'VaultUserOperations': {
		properties: {

		},
		methods: {
			'GetUserList': 'MFilesAPI.KeyNamePairs',
			'GetUserAccounts': 'MFilesAPI.UserAccounts',
			'ModifyUserAccount': 'System.Void',
			'AddUserAccount_DEPRECATED': 'System.Int32',
			'RemoveUserAccount': 'System.Void',
			'GetUserAccount': 'MFilesAPI.UserAccount',
			'AddUserAccount': 'MFilesAPI.UserAccount'
		}
	},
	'KeyNamePairs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.KeyNamePair'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'KeyNamePair': {
		properties: {
			'Key': 'System.Int32',
			'Name': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.KeyNamePair'
		}
	},
	'UserAccounts': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.UserAccount'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'UserAccount': {
		properties: {
			'ID': 'System.Int32',
			'LoginName': 'System.String',
			'VaultRoles': 'MFilesAPI.MFUserAccountVaultRole',
			'InternalUser': 'System.Boolean',
			'AccessControlList': 'MFilesAPI.AccessControlList',
			'VaultLanguage': 'System.Int16'
		},
		methods: {
			'AddVaultRoles': 'System.Void',
			'RemoveVaultRoles': 'System.Void'
		}
	},
	'VaultUserGroupOperations': {
		properties: {

		},
		methods: {
			'GetUserGroups': 'MFilesAPI.UserGroups',
			'GetUserGroupList': 'MFilesAPI.KeyNamePairs',
			'AddUserGroup_DEPRECATED': 'MFilesAPI.UserGroup',
			'RemoveUserGroupAdmin': 'System.Void',
			'UpdateUserGroup_DEPRECATED': 'System.Void',
			'GetUserGroupsAdmin': 'MFilesAPI.UserGroupsAdmin',
			'AddUserGroupAdmin': 'MFilesAPI.UserGroupAdmin',
			'UpdateUserGroupAdmin': 'System.Void',
			'GetUserGroup': 'MFilesAPI.UserGroup',
			'GetUserGroupAdmin': 'MFilesAPI.UserGroupAdmin',
			'GetGroupsOfUserOrGroup': 'MFilesAPI.UserGroups',
			'GetUserGroupIDByAlias': 'System.Int32'
		}
	},
	'UserGroups': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.UserGroup'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'UserGroup': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String',
			'Members': 'MFilesAPI.IDs',
			'AccessControlList_DEPRECATED': 'MFilesAPI.AccessControlList'
		},
		methods: {
			'AddMember': 'System.Void',
			'RemoveMember': 'System.Void',
			'Clone': 'MFilesAPI.UserGroup'
		}
	},
	'UserGroupsAdmin': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.UserGroupAdmin'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'UserGroupAdmin': {
		properties: {
			'UserGroup': 'MFilesAPI.UserGroup',
			'AccessControlList': 'MFilesAPI.AccessControlList',
			'SemanticAliases': 'MFilesAPI.SemanticAliases'
		},
		methods: {
			'Clone': 'MFilesAPI.UserGroupAdmin'
		}
	},
	'VaultNamedACLOperations': {
		properties: {

		},
		methods: {
			'GetNamedACLs': 'MFilesAPI.NamedACLs',
			'AddNamedACL_DEPRECATED': 'MFilesAPI.NamedACL',
			'RemoveNamedACLAdmin': 'System.Void',
			'UpdateNamedACL_DEPRECATED': 'System.Void',
			'GetNamedACLsAdmin': 'MFilesAPI.NamedACLsAdmin',
			'AddNamedACLAdmin': 'MFilesAPI.NamedACLAdmin',
			'UpdateNamedACLAdmin': 'System.Void',
			'GetNamedACL': 'MFilesAPI.NamedACL',
			'GetNamedACLAdmin': 'MFilesAPI.NamedACLAdmin',
			'IsNamedACLUsedInAutomaticPermissionsAdmin': 'System.Boolean',
			'UpdateNamedACLWithPropagationAdmin': 'System.Void',
			'RemoveNamedACLWithPropagationAdmin': 'System.Void',
			'GetNamedACLsByTypeAdmin': 'MFilesAPI.NamedACLsAdmin',
			'GetMatchingNamedACLForAccessControlList': 'MFilesAPI.NamedACL',
			'GetMatchingNamedACLForAccessControlListComponent': 'MFilesAPI.NamedACL',
			'GetNamedACLsWithRefresh': 'MFilesAPI.NamedACLs',
			'GetNamedACLWithRefresh': 'MFilesAPI.NamedACL',
			'GetNamedACLIDByAlias': 'System.Int32'
		}
	},
	'NamedACLs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.NamedACL'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'NamedACLsAdmin': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.NamedACLAdmin'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'NamedACLAdmin': {
		properties: {
			'NamedACL': 'MFilesAPI.NamedACL',
			'AccessControlListForNamedACL': 'MFilesAPI.AccessControlList',
			'SemanticAliases': 'MFilesAPI.SemanticAliases'
		},
		methods: {
			'Clone': 'MFilesAPI.NamedACLAdmin'
		}
	},
	'VaultTraditionalFolderOperations': {
		properties: {

		},
		methods: {
			'GetTraditionalFolderContents': 'MFilesAPI.TraditionalFolderContents'
		}
	},
	'TraditionalFolderContents': {
		properties: {
			'ID': 'System.Int32',
			'TraditionalFolders': 'MFilesAPI.TraditionalFolders',
			'ObjectVersions': 'MFilesAPI.ObjectVersions'
		},
		methods: {

		}
	},
	'TraditionalFolders': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.TraditionalFolder'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'TraditionalFolder': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String',
			'HasChildren': 'System.Boolean'
		},
		methods: {

		}
	},
	'SessionInfo': {
		properties: {
			'UserID': 'System.Int32',
			'InternalUser': 'System.Boolean',
			'ProductMode': 'MFilesAPI.MFProductMode',
			'LocalComputerName': 'System.String',
			'VaultGUID': 'System.String',
			'AccountName': 'System.String',
			'AuthenticationType': 'MFilesAPI.MFAuthType',
			'ServerVersion': 'System.UInt64',
			'Language': 'System.Int16',
			'KeepAliveIntervalInSeconds': 'System.Int32',
			'UserAndSubstitutedByMe': 'MFilesAPI.UserOrUserGroupIDs',
			'UserAndGroupMemberships': 'MFilesAPI.UserOrUserGroupIDs',
			'CanManageCommonViews': 'System.Boolean',
			'CanManageCommonUISettings': 'System.Boolean',
			'CanManageTraditionalFolders': 'System.Boolean',
			'CanCreateObjects': 'System.Boolean',
			'CanSeeAllObjects': 'System.Boolean',
			'CanSeeDeletedObjects': 'System.Boolean',
			'LicenseAllowsModifications': 'System.Boolean',
			'CanForceUndoCheckout': 'System.Boolean',
			'TimeZoneInfo': 'MFilesAPI.TimeZoneInformation',
			'CanMaterializeViews': 'System.Boolean',
			'ACLMode': 'MFilesAPI.MFACLMode'
		},
		methods: {
			'CheckPropertyDefAccess': 'System.Boolean',
			'CheckObjectTypeAccess': 'System.Boolean',
			'IsLoggedOnUserSubstituteOfUser': 'System.Boolean',
			'CheckObjectAccess': 'System.Boolean',
			'CheckVaultAccess': 'System.Boolean'
		}
	},
	'TimeZoneInformation': {
		properties: {
			'StandardName': 'System.String'
		},
		methods: {
			'LoadWithCurrentTimeZone': 'System.Void',
			'LoadTimeZoneByName': 'System.Void',
			'LoadTimeZoneByIndex': 'System.Void'
		}
	},
	'MFilesVersion': {
		properties: {
			'Major': 'System.Int32',
			'Minor': 'System.Int32',
			'Build': 'System.Int32',
			'Patch': 'System.Int32',
			'SoftwarePlatform': 'MFilesAPI.MFSoftwarePlatformType',
			'Display': 'System.String'
		},
		methods: {
			'CompareTo': 'System.Int32',
			'Clone': 'MFilesAPI.MFilesVersion'
		}
	},
	'VaultClientOperations': {
		properties: {

		},
		methods: {
			'IsOffline': 'System.Boolean',
			'IsOnline': 'System.Boolean',
			'SetVaultToOffline': 'MFilesAPI.MFOfflineTransitionResultFlags',
			'SetVaultToOnline': 'MFilesAPI.MFOnlineTransitionResultFlags',
			'DisableCheckInReminderForCallingProcess': 'System.Void',
			'EnableCheckInReminderForCallingProcess': 'System.Void'
		}
	},
	'VaultObjectSearchOperations': {
		properties: {

		},
		methods: {
			'SearchForObjectsByExportedSearchConditionsXML': 'MFilesAPI.XMLSearchResult',
			'SearchForObjectsByExportedSearchConditions': 'MFilesAPI.ObjectSearchResults',
			'SearchForObjectsByString': 'MFilesAPI.ObjectSearchResults',
			'SearchForObjectsByCondition': 'MFilesAPI.ObjectSearchResults',
			'SearchForObjectsByConditionsXML': 'MFilesAPI.XMLSearchResult',
			'SearchForObjectsByConditions': 'MFilesAPI.ObjectSearchResults',
			'SearchForObjectsByConditionsEx': 'MFilesAPI.ObjectSearchResults',
			'GetObjectsInPath': 'MFilesAPI.ObjectSearchResults',
			'FindObjectVersionAndProperties': 'MFilesAPI.ObjectVersionAndProperties',
			'FindFile': 'MFilesAPI.ObjectVersionFile',
			'IsObjectPathInMFiles': 'System.Boolean'
		}
	},
	'XMLSearchResult': {
		properties: {
			'SearchResult': 'System.String',
			'MoreResults': 'System.Boolean'
		},
		methods: {

		}
	},
	'ObjectSearchResults': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjectVersion',
			'MoreResults': 'System.Boolean',
			'ObjectVersions': 'MFilesAPI.ObjectVersions'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Sort': 'System.Void',
			'GetAsObjectVersions': 'MFilesAPI.ObjectVersions',
			'GetScoreOfObject': 'System.Int32',
			'ScoreAt': 'System.Int32',
			'SortByScore': 'System.Void'
		}
	},
	'VaultManagementOperations': {
		properties: {

		},
		methods: {
			'RebuildFullTextSearchIndex': 'System.Void',
			'UpdateVaultProperties': 'System.Void',
			'GetEventHandlers': 'MFilesAPI.EventHandlers',
			'SetEventHandlers': 'System.Void',
			'ExportContent': 'System.Void',
			'ArchiveOldVersions': 'System.Void',
			'ImportContent': 'System.Void',
			'VerifyVault': 'MFilesAPI.VerifyVaultJobOutput'
		}
	},
	'VaultProperties': {
		properties: {
			'VaultGUID': 'System.String',
			'DisplayName': 'System.String',
			'MainDataFolder': 'System.String',
			'FullTextSearchLanguage': 'System.String',
			'SQLDatabase': 'MFilesAPI.SQLDatabase',
			'SeparateLocationForFileData': 'MFilesAPI.AdditionalFolders',
			'ExtendedMetadataDrivenPermissions': 'System.Boolean',
			'FileDataStorageType': 'MFilesAPI.MFFileDataStorage',
			'FileDataConnectionString': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.VaultProperties'
		}
	},
	'SQLDatabase': {
		properties: {
			'Engine': 'MFilesAPI.MFDBEngine',
			'Server': 'System.String',
			'Name': 'System.String',
			'Impersonation': 'MFilesAPI.Impersonation'
		},
		methods: {
			'Clone': 'MFilesAPI.SQLDatabase'
		}
	},
	'Impersonation': {
		properties: {
			'ImpersonationType': 'MFilesAPI.MFImpersonationType',
			'Account': 'System.String',
			'Password': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.Impersonation'
		}
	},
	'AdditionalFolders': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.AdditionalFolder'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.AdditionalFolders'
		}
	},
	'AdditionalFolder': {
		properties: {
			'Folder': 'System.String',
			'LimitInMB': 'System.Int32',
			'Impersonation': 'MFilesAPI.Impersonation'
		},
		methods: {
			'Clone': 'MFilesAPI.AdditionalFolder'
		}
	},
	'EventHandlers': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.EventHandler'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.EventHandlers'
		}
	},
	'EventHandler': {
		properties: {
			'EventType': 'MFilesAPI.MFEventHandlerType',
			'Active': 'System.Boolean',
			'Description': 'System.String',
			'VBScript': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.EventHandler'
		}
	},
	'ExportContentJob': {
		properties: {
			'VaultGUID': 'System.String',
			'TargetFile': 'System.String',
			'Flags': 'MFilesAPI.MFExportContentFlag',
			'SearchConditions': 'MFilesAPI.SearchConditions',
			'IgnoreVersionsBefore': 'MFilesAPI.Timestamp',
			'Impersonation': 'MFilesAPI.Impersonation',
			'UseSearchConditions': 'System.Boolean',
			'UseIgnoreChangesBefore': 'System.Boolean',
			'IgnoreChangesBefore': 'MFilesAPI.Timestamp',
			'TargetLocation': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.ExportContentJob'
		}
	},
	'ArchiveOldVersionsJob': {
		properties: {
			'TargetFile': 'System.String',
			'Impersonation': 'MFilesAPI.Impersonation',
			'Flags': 'MFilesAPI.MFExportContentFlag',
			'MarkedForArchiving': 'System.Boolean',
			'UseCheckedInBefore': 'System.Boolean',
			'CheckedInBefore': 'MFilesAPI.Timestamp',
			'UseAtLeastNVersionsOlder': 'System.Boolean',
			'AtLeastNVersionsOlder': 'System.Int32',
			'UseAtLeastNDaysOlder': 'System.Boolean',
			'AtLeastNDaysOlder': 'System.Int32',
			'NoVersionTag': 'System.Boolean',
			'TargetLocation': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.ArchiveOldVersionsJob'
		}
	},
	'ImportContentJob': {
		properties: {
			'VaultGUID': 'System.String',
			'SourceFile': 'System.String',
			'Flags': 'MFilesAPI.MFImportContentFlag',
			'Permissions': 'MFilesAPI.AccessControlList',
			'Impersonation': 'MFilesAPI.Impersonation',
			'UsePermissions': 'System.Boolean',
			'SourceLocation': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.ImportContentJob'
		}
	},
	'VerifyVaultJob': {
		properties: {
			'VaultGUID': 'System.String',
			'FixErrors': 'System.Boolean',
			'VerifyFileContents': 'System.Boolean'
		},
		methods: {
			'Set': 'System.Void',
			'Clone': 'MFilesAPI.VerifyVaultJob',
			'GetNumberOfSteps': 'System.Int32',
			'GetOneBasedIndexOfStep': 'System.Int32',
			'GetStepProgressText': 'System.String'
		}
	},
	'VerifyVaultJobOutput': {
		properties: {
			'Errors': 'MFilesAPI.Strings'
		},
		methods: {

		}
	},
	'VaultUserSettingOperations': {
		properties: {

		},
		methods: {
			'GetSubstituteUsers': 'MFilesAPI.UserOrUserGroupIDs',
			'SetSubstituteUsers': 'System.Void',
			'GetVaultLanguage': 'System.Int16',
			'ChangeVaultLanguage': 'System.Void'
		}
	},
	'Languages': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.Language'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'Language': {
		properties: {
			'ID': 'System.Int16',
			'Name': 'System.String',
			'LanguageCode': 'System.String'
		},
		methods: {

		}
	},
	'VaultNamedValueStorageOperations': {
		properties: {

		},
		methods: {
			'GetNamedValues': 'MFilesAPI.NamedValues',
			'SetNamedValues': 'System.Void',
			'RemoveNamedValues': 'System.Void'
		}
	},
	'VaultDataSetOperations': {
		properties: {

		},
		methods: {
			'GetDataSets': 'MFilesAPI.DataSets',
			'GetReportAccessCredentials': 'MFilesAPI.ReportAccessCredentials',
			'StartDataSetExport': 'System.Void',
			'GetDataSetExportingStatus': 'MFilesAPI.DataSetExportingStatus'
		}
	},
	'DataSets': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.DataSet'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Clone': 'MFilesAPI.DataSets'
		}
	},
	'DataSet': {
		properties: {
			'ID': 'System.Int32',
			'Name': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.DataSet'
		}
	},
	'ReportAccessCredentials': {
		properties: {
			'UserName': 'System.String',
			'Domain': 'System.String',
			'Password': 'System.String',
			'ExtAccount': 'System.Boolean'
		},
		methods: {

		}
	},
	'DataSetExportingStatus': {
		properties: {
			'IsExporting': 'System.Boolean',
			'CurrentServerTime': 'MFilesAPI.Timestamp',
			'LatestActivity': 'MFilesAPI.Timestamp'
		},
		methods: {

		}
	},
	'VaultElectronicSignatureOperations': {
		properties: {

		},
		methods: {
			'AddEmptySignature': 'MFilesAPI.ObjectVersionAndProperties',
			'AddEmptySignatures': 'MFilesAPI.ObjectVersionAndProperties',
			'DisconnectSignature': 'MFilesAPI.ObjectVersionAndProperties',
			'DisconnectSignatures': 'MFilesAPI.ObjectVersionAndProperties'
		}
	},
	'VaultScheduledJobManagementOperations': {
		properties: {

		},
		methods: {
			'GetScheduledJobs': 'MFilesAPI.ScheduledJobs',
			'GetScheduledJob': 'MFilesAPI.ScheduledJob',
			'GetScheduledJobRunInfo': 'MFilesAPI.ScheduledJobRunInfo',
			'StartScheduledJob': 'System.Void',
			'CancelScheduledJob': 'System.Void',
			'AddScheduledJob': 'System.Int32',
			'ModifyScheduledJob': 'System.Void',
			'RemoveScheduledJob': 'System.Void'
		}
	},
	'ScheduledJobs': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ScheduledJob'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'ScheduledJob': {
		properties: {
			'ID': 'System.Int32',
			'JobName': 'System.String',
			'Enabled': 'System.Boolean',
			'Temporary': 'System.Boolean',
			'JobType': 'MFilesAPI.MFScheduledJobType',
			'OptimizeVaultJob': 'MFilesAPI.OptimizeVaultJob',
			'BackupVaultJob': 'MFilesAPI.BackupJob',
			'Triggers': 'MFilesAPI.ScheduledJobTriggers',
			'ExportContentJob': 'MFilesAPI.ExportContentJob',
			'ImportContentJob': 'MFilesAPI.ImportContentJob'
		},
		methods: {
			'SetOptimizeVaultJob': 'System.Void',
			'SetBackupVaultJob': 'System.Void',
			'SetExportContentJob': 'System.Void',
			'SetImportContentJob': 'System.Void'
		}
	},
	'OptimizeVaultJob': {
		properties: {
			'Thorough': 'System.Boolean',
			'VaultGUID': 'System.String',
			'GarbageCollectFiles': 'System.Boolean'
		},
		methods: {
			'Set': 'System.Void',
			'GetNumberOfSteps': 'System.Int32',
			'GetOneBasedIndexOfStep': 'System.Int32',
			'GetStepProgressText': 'System.String'
		}
	},
	'BackupJob': {
		properties: {
			'VaultGUID': 'System.String',
			'TargetFile': 'System.String',
			'OverwriteExistingFiles': 'System.Boolean',
			'BackupType': 'MFilesAPI.MFBackupType',
			'FileSizeLimitInMB': 'System.Int32',
			'Impersonation': 'MFilesAPI.Impersonation'
		},
		methods: {
			'Clone': 'MFilesAPI.BackupJob'
		}
	},
	'ScheduledJobTriggers': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ScheduledJobTrigger'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.ScheduledJobTriggers'
		}
	},
	'ScheduledJobTrigger': {
		properties: {
			'BeginYear': 'System.Int32',
			'BeginMonth': 'System.Int32',
			'BeginDay': 'System.Int32',
			'EndYear': 'System.Int32',
			'EndMonth': 'System.Int32',
			'EndDay': 'System.Int32',
			'StartHour': 'System.Int32',
			'StartMinute': 'System.Int32',
			'ValidEndDate': 'System.Boolean',
			'Type': 'MFilesAPI.TriggerType'
		},
		methods: {
			'Clone': 'MFilesAPI.ScheduledJobTrigger'
		}
	},
	'TriggerType': {
		properties: {
			'Type': 'MFilesAPI.MFTriggerType',
			'Daily': 'MFilesAPI.DailyTrigger',
			'Weekly': 'MFilesAPI.WeeklyTrigger',
			'MonthlyDate': 'MFilesAPI.MonthlyDateTrigger',
			'MonthlyDOW': 'MFilesAPI.MonthlyDOWTrigger'
		},
		methods: {
			'Clone': 'MFilesAPI.TriggerType',
			'SetDailyTrigger': 'System.Void',
			'SetWeekly': 'System.Void',
			'SetMonthlyDate': 'System.Void',
			'SetMonthlyDOW': 'System.Void'
		}
	},
	'DailyTrigger': {
		properties: {
			'DaysInterval': 'System.Int32'
		},
		methods: {

		}
	},
	'WeeklyTrigger': {
		properties: {
			'WeeksInterval': 'System.Int32',
			'DaysOfTheWeek': 'MFilesAPI.MFTriggerWeekDay'
		},
		methods: {

		}
	},
	'MonthlyDateTrigger': {
		properties: {
			'Days': 'System.Int32',
			'Months': 'MFilesAPI.MFTriggerMonth'
		},
		methods: {

		}
	},
	'MonthlyDOWTrigger': {
		properties: {
			'WhichWeek': 'MFilesAPI.MFTriggerWeekOfMonth',
			'DaysOfTheWeek': 'MFilesAPI.MFTriggerWeekDay',
			'Months': 'MFilesAPI.MFTriggerMonth'
		},
		methods: {

		}
	},
	'ScheduledJobRunInfo': {
		properties: {
			'ScheduledJobOutputInfo': 'MFilesAPI.ScheduledJobOutputInfo',
			'Running': 'System.Boolean',
			'NextRun': 'MFilesAPI.Timestamp',
			'LastRun': 'MFilesAPI.Timestamp',
			'LastRunSucceeded': 'System.Boolean',
			'LastRunErrors': 'System.String',
			'Cancelled': 'System.Boolean',
			'CurrentStep': 'System.Int32',
			'StepCompletionPercent': 'System.Int32'
		},
		methods: {

		}
	},
	'ScheduledJobOutputInfo': {
		properties: {
			'ID': 'System.Int32',
			'Message': 'System.String',
			'JobType': 'MFilesAPI.MFScheduledJobType'
		},
		methods: {

		}
	},
	'VaultCustomApplicationManagementOperations': {
		properties: {

		},
		methods: {
			'GetCustomApplications': 'MFilesAPI.CustomApplications',
			'EnableCustomApplication': 'System.Void',
			'InstallCustomApplication': 'System.Void',
			'UninstallCustomApplication': 'System.Void'
		}
	},
	'CustomApplications': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.CustomApplication'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Clone': 'MFilesAPI.CustomApplications'
		}
	},
	'CustomApplication': {
		properties: {
			'ID': 'System.String',
			'Name': 'System.String',
			'Version': 'System.String',
			'Description': 'System.String',
			'Publisher': 'System.String',
			'Enabled': 'System.Boolean',
			'Optional': 'System.Boolean',
			'RequireSystemAccess': 'System.Boolean',
			'ChecksumHash': 'System.String'
		},
		methods: {
			'Clone': 'MFilesAPI.CustomApplication'
		}
	},
	'ServerVaultManagementOperations': {
		properties: {

		},
		methods: {
			'BackupVault': 'System.Void',
			'RestoreVault': 'System.Void',
			'GetBackupFileContents': 'MFilesAPI.VaultProperties',
			'GetVaultProperties': 'MFilesAPI.VaultProperties',
			'DestroyVault': 'System.Void',
			'BringVaultOnline': 'System.Void',
			'TakeVaultOffline': 'System.Void',
			'OptimizeVault': 'System.Void',
			'CopyVault': 'MFilesAPI.CopyVaultJobOutputInfo',
			'AttachVault': 'MFilesAPI.VaultProperties',
			'DetachVault': 'System.Void',
			'CreateNewVault': 'System.String'
		}
	},
	'RestoreJob': {
		properties: {
			'BackupFileFull': 'System.String',
			'BackupFileDifferential': 'System.String',
			'VaultProperties': 'MFilesAPI.VaultProperties',
			'OverwriteExistingFiles': 'System.Boolean',
			'Impersonation': 'MFilesAPI.Impersonation'
		},
		methods: {

		}
	},
	'CopyVaultJob': {
		properties: {
			'VaultGUID': 'System.String',
			'VaultProperties': 'MFilesAPI.VaultProperties',
			'CopyflagAllData': 'System.Boolean',
			'CopyflagValueLists': 'System.Boolean',
			'CopyflagPropertyDefinitions': 'System.Boolean',
			'CopyflagDocumentProfiles': 'System.Boolean',
			'CopyflagUserAccounts': 'System.Boolean',
			'CopyflagViews': 'System.Boolean',
			'CopyflagDocuments': 'System.Boolean',
			'CopyflagValueListContent': 'System.Boolean',
			'CopyflagWorkflows': 'System.Boolean',
			'CopyflagExternalLocations': 'System.Boolean',
			'CopyflagEventLog': 'System.Boolean',
			'CopyflagInternalEventHandlers': 'System.Boolean',
			'CopyflagAllExceptData': 'System.Boolean',
			'CopyflagLanguagesAndTranslations': 'System.Boolean',
			'CopyflagDataSets': 'System.Boolean',
			'CopyflagScheduledExportAndImportJobs': 'System.Boolean',
			'CopyflagApplications': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.CopyVaultJob'
		}
	},
	'CopyVaultJobOutputInfo': {
		properties: {
			'VaultProperties': 'MFilesAPI.VaultProperties'
		},
		methods: {

		}
	},
	'ServerManagementOperations': {
		properties: {

		},
		methods: {
			'BackupMasterDB': 'System.Void',
			'RestoreMasterDB': 'System.Void',
			'ConfigureWebAccessToDefaultWebSite': 'System.Void',
			'ConfigureWebAccessToExistingWebSite': 'System.Void',
			'ConfigureWebAccessToNewWebSite': 'System.Void',
			'ConfigureWebAccessToNewVirtualDirectory': 'System.Void',
			'GetEventHandlers': 'MFilesAPI.EventHandlers',
			'SetEventHandlers': 'System.Void'
		}
	},
	'ServerLicenseManagementOperations': {
		properties: {

		},
		methods: {
			'SetLicenseCodeAndSerialNumber': 'System.Void',
			'GetLicenseStatus': 'MFilesAPI.LicenseStatus'
		}
	},
	'LicenseStatus': {
		properties: {
			'EvaluationMode': 'System.Boolean',
			'Expired': 'System.Boolean',
			'EvaluationDaysLeft': 'System.Int32',
			'SerialNumber': 'System.String',
			'LicenseCode': 'System.String'
		},
		methods: {

		}
	},
	'ServerScheduledJobManagementOperations': {
		properties: {

		},
		methods: {
			'GetScheduledJobs': 'MFilesAPI.ScheduledJobs',
			'GetScheduledJob': 'MFilesAPI.ScheduledJob',
			'GetScheduledJobRunInfo': 'MFilesAPI.ScheduledJobRunInfo',
			'StartScheduledJob': 'System.Void',
			'CancelScheduledJob': 'System.Void',
			'AddScheduledJob': 'System.Int32',
			'ModifyScheduledJob': 'System.Void',
			'RemoveScheduledJob': 'System.Void'
		}
	},
	'ShortcutMappingInfo': {
		properties: {
			'ObjectTypeID': 'System.Int32',
			'ObjectTypeName': 'System.String',
			'ObjectTypeSemanticAliases': 'MFilesAPI.SemanticAliases'
		},
		methods: {

		}
	},
	'LoginAccountPersonalInformation': {
		properties: {
			'AccountName': 'System.String',
			'FullName': 'System.String',
			'Email': 'System.String'
		},
		methods: {

		}
	},
	'LoginAccount': {
		properties: {
			'AccountName': 'System.String',
			'FullName': 'System.String',
			'EmailAddress': 'System.String',
			'DomainName': 'System.String',
			'UserName': 'System.String',
			'AccountType': 'MFilesAPI.MFLoginAccountType',
			'LicenseType': 'MFilesAPI.MFLicenseType',
			'ServerRoles': 'MFilesAPI.MFLoginServerRole',
			'Enabled': 'System.Boolean'
		},
		methods: {
			'Set': 'System.Void',
			'Clone': 'MFilesAPI.LoginAccount'
		}
	},
	'LoginAccounts': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.LoginAccount'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator'
		}
	},
	'VaultConnection': {
		properties: {
			'Name': 'System.String',
			'ProtocolSequence': 'System.String',
			'NetworkAddress': 'System.String',
			'Endpoint': 'System.String',
			'ServerVaultGUID': 'System.String',
			'ServerVaultName': 'System.String',
			'UserName': 'System.String',
			'Password': 'System.String',
			'Domain': 'System.String',
			'AuthType': 'MFilesAPI.MFAuthType',
			'AutoLogin': 'System.Boolean',
			'UserSpecific': 'System.Boolean',
			'PredefinedCredentials': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.VaultConnection',
			'LogInAs': 'MFilesAPI.Vault',
			'LogInAsUser': 'MFilesAPI.Vault',
			'TestConnectionToVaultSilent': 'MFilesAPI.MFVaultConnectionTestResult',
			'BindToVault': 'MFilesAPI.Vault',
			'GetGUID': 'System.String',
			'IsLoggedIn': 'System.Boolean',
			'TestConnectionToVault': 'MFilesAPI.MFVaultConnectionTestResult'
		}
	},
	'VaultConnections': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.VaultConnection'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'GetVaultConnectionByName': 'MFilesAPI.VaultConnection',
			'GetVaultConnectionIndexByName': 'System.Int32'
		}
	},
	'VaultOnServer': {
		properties: {
			'Name': 'System.String',
			'GUID': 'System.String'
		},
		methods: {
			'LogInAsUser': 'MFilesAPI.Vault',
			'LogIn': 'MFilesAPI.Vault',
			'LogInAsUserWithSPN': 'MFilesAPI.Vault'
		}
	},
	'VaultsOnServer': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.VaultOnServer'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'GetVaultByName': 'MFilesAPI.VaultOnServer',
			'GetVaultIndexByName': 'System.Int32',
			'GetVaultByGUID': 'MFilesAPI.VaultOnServer',
			'GetVaultIndexByGUID': 'System.Int32'
		}
	},
	'MFilesClientApplication': {
		properties: {

		},
		methods: {
			'GetVaultConnections': 'MFilesAPI.VaultConnections',
			'GetVaultConnectionsWithGUID': 'MFilesAPI.VaultConnections',
			'GetVaultConnection': 'MFilesAPI.VaultConnection',
			'FindObjectVersionAndProperties': 'MFilesAPI.ObjectVersionAndProperties',
			'FindFile': 'MFilesAPI.ObjectVersionFile',
			'IsObjectPathInMFiles': 'System.Boolean',
			'GetDriveLetter': 'System.String',
			'GetClientVersion': 'MFilesAPI.MFilesVersion',
			'GetAPIVersion': 'MFilesAPI.MFilesVersion',
			'BindToVault': 'MFilesAPI.Vault',
			'TestConnectionToServer': 'System.Int32',
			'ShowBalloonTip': 'System.Void',
			'AddVaultConnection': 'System.Void',
			'RemoveVaultConnection': 'System.Void',
			'LogInAs': 'MFilesAPI.Vault',
			'LogInAsUser': 'MFilesAPI.Vault'
		}
	},
	'Expressions': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.Expression'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void'
		}
	},
	'ServerLoginAccountOperations': {
		properties: {

		},
		methods: {
			'GetLoginAccounts': 'MFilesAPI.LoginAccounts',
			'GetLoginAccount': 'MFilesAPI.LoginAccount',
			'ModifyLoginAccount': 'System.Void',
			'RemoveLoginAccount': 'System.Void',
			'AddLoginAccount': 'System.Void',
			'GetPersonalInformationFromDomain': 'MFilesAPI.LoginAccountPersonalInformation',
			'UpdateLoginPassword': 'System.Void',
			'GetPasswordHash': 'System.String',
			'SetPasswordHash': 'System.Void',
			'GetLoginAccountsWithSessions': 'MFilesAPI.LoginAccounts',
			'ForceLogout': 'System.Void'
		}
	},
	'MFilesServerApplication': {
		properties: {
			'LoginAccountOperations': 'MFilesAPI.ServerLoginAccountOperations',
			'VaultManagementOperations': 'MFilesAPI.ServerVaultManagementOperations',
			'ServerManagementOperations': 'MFilesAPI.ServerManagementOperations',
			'LicenseManagementOperations': 'MFilesAPI.ServerLicenseManagementOperations',
			'ScheduledJobManagementOperations': 'MFilesAPI.ServerScheduledJobManagementOperations'
		},
		methods: {
			'Connect': 'MFilesAPI.MFServerConnection',
			'GetVaults': 'MFilesAPI.VaultsOnServer',
			'GetAPIVersion': 'MFilesAPI.MFilesVersion',
			'GetServerVersion': 'MFilesAPI.MFilesVersion',
			'Disconnect': 'System.Void',
			'TestConnectionToServer': 'System.Int32',
			'LogInAsUserToVault': 'MFilesAPI.Vault',
			'LogInToVault': 'MFilesAPI.Vault',
			'ConnectWithTimeZone': 'MFilesAPI.MFServerConnection',
			'ConnectAdministrative': 'MFilesAPI.MFServerConnection',
			'GetOnlineVaults': 'MFilesAPI.VaultsOnServer',
			'LogInAsUserToVaultWithSPN': 'MFilesAPI.Vault',
			'ConnectWithSPN': 'MFilesAPI.MFServerConnection',
			'ConnectWithTimeZoneAndSPN': 'MFilesAPI.MFServerConnection',
			'ConnectAdministrativeWithSPN': 'MFilesAPI.MFServerConnection'
		}
	},
	'MFResourceManager': {
		properties: {

		},
		methods: {
			'LoadResourceString': 'System.String',
			'GetLocaleSpecificDateFormat': 'System.String',
			'GetUICultures': 'MFilesAPI.Strings'
		}
	},
	'ObjectFileAndObjVer': {
		properties: {
			'ObjectFile': 'MFilesAPI.ObjectFile',
			'ObjVer': 'MFilesAPI.ObjVer'
		},
		methods: {
			'Clone': 'MFilesAPI.ObjectFileAndObjVer'
		}
	},
	'ObjectFileAndObjVerOfMultipleFiles': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'MFilesAPI.ObjectFileAndObjVer'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.ObjectFileAndObjVerOfMultipleFiles'
		}
	},
	'Number': {
		properties: {
			'Value': 'System.Int32'
		},
		methods: {
			'Clone': 'MFilesAPI.Number'
		}
	},
	'BooleanValue': {
		properties: {
			'Value': 'System.Boolean'
		},
		methods: {
			'Clone': 'MFilesAPI.BooleanValue'
		}
	},
	'DsClass': {
		properties: {
			'Count': 'System.Int32',
			'Item': 'System.Int32'
		},
		methods: {
			'GetEnumerator': 'System.Collections.IEnumerator',
			'Add': 'System.Void',
			'Remove': 'System.Void',
			'Clone': 'MFilesAPI.IDs',
			'IndexOf': 'System.Int32',
			'RemoveAll': 'System.Int32',
			'GetLifetimeService': 'System.Object',
			'InitializeLifetimeService': 'System.Object',
			'CreateObjRef': 'System.Runtime.Remoting.ObjRef',
			'ToString': 'System.String',
			'Equals': 'System.Boolean',
			'GetHashCode': 'System.Int32',
			'GetType': 'System.Type'
		}
	}
}