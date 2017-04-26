/*!
 * 工作流MFiles入口，从MFiles获取数据等相关接口 
 **/
var CC = CC || {};
(function (u, undefined) {      
    var ForReading = 1, ForWriting = 2, ForAppending = 8;
    function jslog(content){
        // var fso = new ActiveXObject("Scripting.FileSystemObject");
        // var fn="c:\\js.log.txt";
        // var ts = fso.OpenTextFile(fn, ForAppending,true);
        // var myDate = new Date();
        // ts.Write(myDate.toString( )+" - "+content+"\n");
        // ts.Close();
    }

    var mfflow = {
        //当选中对象时，响应函数入口
        selectionChangedHandler: function(shellFrame) {
          jslog("in selectionChangedHandler")
            var that = this;
            return function(selItems) {
                 jslog("in selectionChangedHandler 11 ")
                if (!shellFrame.Listing || !shellFrame.Listing.CurrentSelection) return;
  jslog("in selectionChangedHandler 22")
                that.selectWorkFlowObjVersion(shellFrame, selItems);
            };
        },

        getWorkflow: function(vault, workflowId) {
            var eventMethodName = "getWorkFlowStates";
            var inputValue = workflowId.toString();
            jslog("wfid="+ inputValue);
            var wfd = vault.ExtensionMethodOperations.ExecuteVaultExtensionMethod(
                eventMethodName, inputValue);
                 jslog("in hh")
            return wfd;
        },

        getFlowChangedVersins: function(shellFrame, flowVersions) {
            //排序
            flowVersions.sort(function(a, b) {
                return a.versionId - b.versionId;
            });
            var flowChangedVns = [];
            var tempStateId = -1;
            //var tipInfo = "";
            for (var i = 0; i < flowVersions.length; i++) {
                flowChangedVns.push(flowVersions[i]);
            }

            return flowChangedVns;
        },

        _getHistoryVersionFlows: function(vault, objVersion) {
            var versionFlows = [];
            var objVersions = vault.ObjectOperations.GetHistory(objVersion.ObjVer.ObjID);

            for (var i = 1; i <= objVersions.Count; i++) {
                var versionState = MF.vault.getWorkflowState(vault, objVersions.Item(i).ObjVer);
                var index = versionFlows.length - 1;
                if (index > -1 && versionFlows[index].stateId === versionState.stateId
                    && versionFlows[index].operatedBy === versionState.operatedBy) {
                    versionFlows[index] = versionState;
                } else {
                    versionFlows.push(versionState);
                }
            }
            return versionFlows;
        },

        isTemplate: function(pvs) {
            //MFBuiltInPropertyDefIsTemplate
            var index = pvs.IndexOf(MFBuiltInPropertyDefIsTemplate);
            if (index !== -1) {
                var tv = pvs.SearchForProperty(MFBuiltInPropertyDefIsTemplate).Value;
                if (tv.IsNULL()) return false;
                return tv.Value;
            }
            return false;
        },
        //多级审核
        _getMultiLevel: function(vault, properties) {
            var res = {
                "isMultiCheck": false,
                "maxLevel": null//"五级审核"
            }
            var multCheckPropId = MF.alias.propertyDef(vault, md.multiCheckFlowDoc.propDefs.PropMultiChecker);
            if (properties.IndexOf(multCheckPropId) === -1) return res;
            var mcTvalue = properties.SearchForProperty(multCheckPropId).Value;
            if(mcTvalue.IsNULL() === false) {
                var mcId = mcTvalue.GetLookupID();
                var mcType = MF.alias.objectType(vault, md.multiChecker.typeAlias);
                var propIdC1 = MF.alias.propertyDef(vault, md.multiChecker.propDefs.PropCheck1);
                var propIdC2 = MF.alias.propertyDef(vault, md.multiChecker.propDefs.PropCheck2);
                var propIdC3 = MF.alias.propertyDef(vault, md.multiChecker.propDefs.PropCheck3);
                var propIdC4 = MF.alias.propertyDef(vault, md.multiChecker.propDefs.PropCheck4);
                var propIdC5 = MF.alias.propertyDef(vault, md.multiChecker.propDefs.PropCheck5);
                var propIdC6 = MF.alias.propertyDef(vault, md.multiChecker.propDefs.PropCheck6);
                var propIdC7 = MF.alias.propertyDef(vault, md.multiChecker.propDefs.PropCheck7);
                var propIdC8 = MF.alias.propertyDef(vault, md.multiChecker.propDefs.PropCheck8);
                var propIdC9 = MF.alias.propertyDef(vault, md.multiChecker.propDefs.PropCheck9);
                var propIdC10 = MF.alias.propertyDef(vault, md.multiChecker.propDefs.PropCheck10);
                var oObjVer = new MFiles.ObjVer();
                oObjVer.SetIDs(mcType, mcId, -1);
                var mcProps = vault.ObjectOperations.GetObjectVersionAndProperties(oObjVer, false).Properties;
                if (mcProps.SearchForProperty(propIdC1).Value.DisplayValue) {
                    res.isMultiCheck = true;
                    res.maxLevel = "一级审核";
                } else {
                    return res;
                }
                if (mcProps.SearchForProperty(propIdC2).Value.DisplayValue) {
                    res.isMultiCheck = true;
                    res.maxLevel = "二级审核";
                } else {
                    return res;
                }
                if (mcProps.SearchForProperty(propIdC3).Value.DisplayValue) {
                    res.isMultiCheck = true;
                    res.maxLevel = "三级审核";
                } else {
                    return res;
                }
                if (mcProps.SearchForProperty(propIdC4).Value.DisplayValue) {
                    res.isMultiCheck = true;
                    res.maxLevel = "四级审核";
                }
                else {
                    return res;
                }
                if (mcProps.SearchForProperty(propIdC5).Value.DisplayValue) {
                    res.isMultiCheck = true;
                    res.maxLevel = "五级审核";
                }
                else {
                    return res;
                }
                if (mcProps.SearchForProperty(propIdC6).Value.DisplayValue) {
                    res.isMultiCheck = true;
                    res.maxLevel = "六级审核";
                } else {
                    return res;
                }
                if (mcProps.SearchForProperty(propIdC7).Value.DisplayValue) {
                    res.isMultiCheck = true;
                    res.maxLevel = "七级审核";
                } else {
                    return res;
                }
                if (mcProps.SearchForProperty(propIdC8).Value.DisplayValue) {
                    res.isMultiCheck = true;
                    res.maxLevel = "八级审核";
                } else {
                    return res;
                }
                if (mcProps.SearchForProperty(propIdC9).Value.DisplayValue) {
                    res.isMultiCheck = true;
                    res.maxLevel = "九级审核";
                } else {
                    return res;
                }
                if (mcProps.SearchForProperty(propIdC10).Value.DisplayValue) {
                    res.isMultiCheck = true;
                    res.maxLevel = "十级审核";
                } else {
                    return res;
                }
            }
            return res;
        },


        
        //选中拥有工作流对象
        selectWorkFlowObjVersion: function(shellFrame, selItems) {
            jslog("in aa")
            if (selItems.ObjectVersions.Count !== 1) {
                this.hideTab(shellFrame, '_workflowpreview');
                  this.hideTab(shellFrame, '_workflowpreviewnew');
                 jslog("in 11")
                return;
            }
             jslog("in bb")
            var itemProperties = selItems.ObjectVersionsAndProperties.Item(1).Properties;
            if (this.isTemplate(itemProperties)) {
                this.hideTab(shellFrame, '_workflowpreview');
                  this.hideTab(shellFrame, '_workflowpreviewnew');
                 jslog("in 22")
                return;
            }
             jslog("in cc")
            var item = selItems.ObjectVersions.Item(1);
            try {
                 jslog("in ccdd")
                var vault = shellFrame.ShellUI.Vault;
 jslog("in dd")
                var wf = vault.ObjectPropertyOperations.GetWorkflowState(item.ObjVer, false);
                if (wf.Workflow.Value.IsNULL()) {
                    this.hideTab(shellFrame, '_workflowpreview');
                      this.hideTab(shellFrame, '_workflowpreviewnew');
                     jslog("in 33")
                    return;
                }
 jslog("in ee"+wf.Workflow.Value.GetValueAsLocalizedText());
                //获取工作流上的所有状态节点及状态转换
                var wfd = this.getWorkflow(vault, wf.Workflow.Value.GetLookupID());
                 jslog("in ff")
                var flows = eval('(' + wfd + ')'); //http://www.cnblogs.com/webflash/archive/2009/11/15/1603537.html
                jslog("in gg")
                if (flows.States.length < 2) {
                    this.hideTab(shellFrame, '_workflowpreview');
                      this.hideTab(shellFrame, '_workflowpreviewnew');
                     jslog("in 44")
                    return;
                }
                 jslog("in 55")
                //获取历史版本
                var versionFlows = this._getHistoryVersionFlows(vault, item);
                
                //进一步提取状态改变的版本
                var flowChangedVersins = this.getFlowChangedVersins(shellFrame, versionFlows);

                //添加右侧的自定义页面展示
                var customData = { workflowAdmin: wfd, versionFlows: flowChangedVersins };
                customData.properties = itemProperties;

                var multiLevel = this._getMultiLevel(vault, itemProperties);
                customData.multiLevel = multiLevel;
                var appTab = null;
                var tabId = '_workflowpreview';
            
                try {
                    appTab = shellFrame.RightPane.GetTab(tabId);
                } catch (e) {
                }
                if (!appTab) {
                    appTab = shellFrame.RightPane.AddTab(tabId, '工作流', "_last");
                }
                if (appTab) {
                    appTab.ShowDashboard('workflow', customData);
                    appTab.Visible = true;
                    appTab.select();
                }

                var newappTab = null;
                var newtabId = '_workflowpreviewnew';            
                try {
                    newappTab = shellFrame.RightPane.GetTab(newtabId);
                } catch (e) {
                }
                if (!newappTab) {
                    newappTab = shellFrame.RightPane.AddTab(newtabId, '工作流历史', "_last");
                }
                if (newappTab) {
                    try{
                        var history = vault.ExtensionMethodOperations.ExecuteVaultExtensionMethod("GetWfnStates", item.ObjVer.Type.toString()+"-"+item.ObjVer.ID.toString());
                        var flowshistory = eval('(' + history + ')'); 
                        newappTab.ShowDashboard('workflownew', { history: flowshistory });
                        newappTab.Visible = true;
                        newappTab.select();
                    } catch (e) {
                        shellFrame.ShellUI.ShowMessage("GetWfnStates"+e);
                    }
                }
            } catch (e) {
                //  shellFrame.ShellUI.ShowMessage("GetTab GetWfnStates"+e);
            }
        },
        hideTab: function (shellFrame, tabId) {
            var appTab = null;
            try {
                appTab = shellFrame.RightPane.GetTab(tabId);
            } catch (e) {
                jslog("c:"+e);
            }
            if (appTab) {
                appTab.Visible = false;
                if (appTab.Selected) {
                    appTab.Unselect();
                }
            }
        }
    }
    u.mfflow = mfflow;
})(CC);