/****************************************
 * 签审流
 * 
 ****************************************/
var CC = CC || {};
(function (u, undefined) {
    var workflow = {

        /*
         *生成描述所有节点的字符串（完整的流程）
         *flowObj：整个节点数据
         */
        combineCompleteNodesStr: function (flowObj, versions) {
            var flowSteps = flowObj.Transitions; //节点流程
            var nodesStates = flowObj.States; //节点名称
            var conditionNodes = this._getConditionNodes(flowSteps); //分支节点
     
            //节点描述
            var str = "";
            var conditionString = [];
            str = "st0=>start: 开始|past[blank]";
            conditionString.push(str);

            for (var m = 0; m < nodesStates.length; m++) {
                var nodeContent = nodesStates[m].Name
                    + "\r\n"
                    + this._getInfoByNodeId(nodesStates, versions, nodesStates[m].Id);
                if (!this.isContainsNode(conditionNodes, nodesStates[m].Id)) {
                    str = this._combineNodeDescription("operation",
                        nodesStates[m].Id,
                        nodeContent,
                        this._getStatusByNodeId(versions, nodesStates[m].Id));
                    conditionString.push(str);
                } else {
                    str = this._combineNodeDescription("condition",
                        nodesStates[m].Id,
                        nodeContent,
                        this._getStatusByNodeId(versions, nodesStates[m].Id));
                    conditionString.push(str);
                }
            }

            return conditionString;
        },

        /*
        *生成描述所有节点的字符串（进行中的流程）
        *flowObj：整个节点数据
        */
        combineOngoingNodesStr: function (flowObj, versions) {
            var flowSteps = flowObj.Transitions; //节点流程
            var nodesStates = flowObj.States; //节点名称
            var conditionNodes = this._getConditionNodes(flowSteps); //分支节点

            //节点描述
            var str = "";
            var conditionString = [];
            str = "st0=>start: 开始|past[blank]";
            conditionString.push(str);

            for (var m = 0; m < nodesStates.length; m++) {
                if (!this.isContainsNode(conditionNodes, nodesStates[m].Id)) {
                    str = this._combineNodeDescription("operation",
                        nodesStates[m].Id,
                        this._getInfoByNodeId(nodesStates, versions, nodesStates[m].Id),
                        this._getStatusByNodeId(versions, nodesStates[m].Id));
                    conditionString.push(str);
                } else {
                    str = this._combineNodeDescription("condition",
                        nodesStates[m].Id,
                        this._getInfoByNodeId(nodesStates, versions, nodesStates[m].Id),
                        this._getStatusByNodeId(versions, nodesStates[m].Id));
                    conditionString.push(str);
                }
            }

            return conditionString;
        },

        /*
         *生成流程步骤（完整的流程）
         *flowObj：整个节点数据
         */
        combineCompleteStepsStr: function (flowObj) {
            var flowSteps = flowObj.Transitions; //节点流程
            flowSteps.sort(function(a,b){
                return a.From-b.From}); //节点排序
            var conditionNodes = this._getConditionNodes(flowSteps); //分支节点

            var str = "";
            var stepString = [];
            for (var k = 0; k < flowSteps.length; k++) {
                str = this._combineStepDescription(flowSteps, flowSteps[k], conditionNodes);
                stepString.push(str);
            }
            return stepString;
        },

        /*
        *生成流程步骤（进行中的流程）
        *flowObj：整个节点数据
        */
        combineOngoingStepsStr: function (flowObj, versions) {
            var flowSteps = flowObj.Transitions; //节点流程
            flowSteps.sort(function(a,b){
                return a.From-b.From}); //节点排序
            var conditionNodes = this._getConditionNodes(flowSteps); //分支节点

            var str = "";
            var stepString = [];
            for (var k = 0; k < flowSteps.length; k++) {
                if (flowSteps[k].From === this._getCurrNodeId(versions)) {
                    break;
                }

                str = this._combineStepDescription(flowSteps, flowSteps[k], conditionNodes);
                stepString.push(str);
            }
            return stepString;
        },

        /****************************************
        * 辅助函数
        ****************************************/
        /*
         *生成节点描述(op101=>operation: 发起审批流程|past)
         *type:节点类型
         *id:节点ID
         *description:节点描述
         *status:节点状态
         *返回节点描述
         */
        _combineNodeDescription: function (type, id, description, status) {
            switch (type) {
                case "start":
                    return "st" + id + "=>" + type + ": " + description + "|" + status;
                case "end":
                    return "e" + id + "=>" + type + ": " + description + "|" + status;;
                case "operation":
                    return "op" + id + "=>" + type + ": " + description + "|" + status;;
                case "condition":
                    return "c" + id + "=>" + type + ": " + description + "|" + status;;
                default:
                    return "op" + id + "=>" + type + ": " + description + "|past";;
            }
        },

        /*生成节点步骤描述（op101->op102|op102->c103）
         *flowSteps:节点步骤
         *currNode:当前节点
         *conditionNodes:有分支节点的节点
         *返回节点步骤描述
         */
        _combineStepDescription: function (flowSteps, currNode, conditionNodes) {
            var fromNodeId = currNode.From;
            var toNodeId = currNode.To;

            var str = "";
            if (fromNodeId === 0) {
                str += "st";
                str += fromNodeId;
            } else {
                if (!this.isContainsNode(conditionNodes, fromNodeId)) {
                    str += "op";
                    str += fromNodeId;
                } else {
                    var conditionStr = this._combineConditionNodeDescription(flowSteps, currNode);
                    if (toNodeId === conditionStr.yes) {
                        str += "c";
                        str += fromNodeId;
                        str += "(yes)";
                    } else {
                        str += "c";
                        str += fromNodeId;
                        str += "(no)";
                    }
                }
            }
            str += "->";

            if (!this.isContainsNode(conditionNodes, toNodeId)) {
                str += "op";
            } else {
                str += "c";
            }
            str += toNodeId;

            return str;
        },
        
        _getToNodes: function(flowSteps, fromNodeId) {
            var childrenNodes = [];
            for (var i = 0; i < flowSteps.length; i++) {
                if (flowSteps[i].From === fromNodeId) {
                    childrenNodes.push(flowSteps[i].To);
                }
            }
            return childrenNodes;
        },
        
        _getFromNode: function(flowSteps, toNodeId) {
            for (var i = 0; i < flowSteps.length; i++) {
                if (flowSteps[i].To === toNodeId) {
                    return flowSteps[i].From;
                }
            }
        },

        /*分支节点描述
         *flowSteps:节点步骤
         *currNode:当前节点
         *返回分支节点yes|no
         */
        _combineConditionNodeDescription: function (flowSteps, currNode) {
            var childrenNodes = this._getToNodes(flowSteps, currNode.From);            
            if (childrenNodes.length === 2) {
                
                var childrenNodeFrom = this._getFromNode(flowSteps, currNode.From);
                
                if (childrenNodeFrom === childrenNodes[0]) {
                    return {
                        yes: childrenNodes[1],
                        no: childrenNodes[0]
                    };
                } else if (childrenNodeFrom === childrenNodes[1]) {
                    return {
                        yes: childrenNodes[0],
                        no: childrenNodes[1]
                    };
                }                
                
                var childs1 = this._getToNodes(flowSteps, childrenNodes[0]);
                var childs2 = this._getToNodes(flowSteps, childrenNodes[1]);
                
                if (childs1.length > 1) {
                    return {
                        yes: childrenNodes[0],
                        no: childrenNodes[1]
                    };
                } else if (childs2.length > 1) {
                    return {
                        yes: childrenNodes[1],
                        no: childrenNodes[0]
                    };
                } else if (childrenNodes[0] > childrenNodes[1]){
                    return {
                        yes: childrenNodes[1],
                        no: childrenNodes[0]
                    };
                } else {
                    return {
                        yes: childrenNodes[0],
                        no: childrenNodes[1]
                    };
                }
            } else {
                return null;
            }
        },

        /*
         *通过Id获取节点名称
         *flowObj:流程信息
         *nodeId:节点ID
         *返回节点名称
         */
        _getNameByNodeId: function (flowObj, nodeId) {
            var name = "";
            var states = flowObj.States;
            for (var i = 0; i < states.length; i++) {
                if (states[i].Id === nodeId) {
                    return states[i].Name;
                }
            }

            return name;
        },

        /*
         *获取节点信息
         *flowObj:流程信息
         */
        _getInfoByNodeId: function (nodesNames, ongoingSteps, nodeId) {
            var info = "";
            for (var i = 0; i < ongoingSteps.length; i++) {
                if (nodeId === ongoingSteps[i].stateId) {
                    if (info !== "") {
                        info += "\r\n";
                    }
                    if (ongoingSteps[i].operatedBy) {
                        info += ongoingSteps[i].operatedBy;
                        info += " | ";
                    }                    
                    info += ongoingSteps[i].operatedTime;
                    if (ongoingSteps[i].comment) {
                        info += "\r\n" + ongoingSteps[i].comment;
                        //info += " | ";
                    }
                }
            }

            return this._formatInfoString(info);
        },

        /*
         *格式化字符串
         */
        _formatInfoString:function(str) {
            if (!str) {
                return str;
            } else {
                var temp = "";
                //"M-Files服务器"则删除
                if (str.indexOf("M-Files服务器") > 0) {
                    return temp;
                }

                //M-Files替换为DBWorld
                temp = str.replace(/M-Files/gi, "DBWorld");

                //添加括号
                temp = "(" + temp;
                temp = temp + ")";
                return temp;
            }
        },

        /*
         *根据节点Id获取节点状态
         *ongoingSteps进行中的步骤
         *nodeId节点id
         *返回节点状态
         */
        _getStatusByNodeId: function (ongoingSteps, nodeId) {
            var str = "past";
            if (!ongoingSteps) {
                return str;
            }

            var currId = ongoingSteps[ongoingSteps.length - 1].stateId;
            if (nodeId === currId) {
                str = "current";
            }
    
            return str;
        },

        /*
         *获取当前步骤节点Id
         */
        _getCurrNodeId: function (ongoingSteps) {
            if (!ongoingSteps) {
                return 0;
            }

            return ongoingSteps[ongoingSteps.length - 1].stateId;
        },

        /*
         *获取有分支的节点
         *nodesStep:节点流程数据
         *返回有分支的节点数组
         */
        _getConditionNodes: function (nodesStep) {
            var condition = []; 
            for (var i = 0; i < nodesStep.length; i++) {
                var front = nodesStep[i].From;
                for (var j = i + 1; j < nodesStep.length; j++) {
                    var next = nodesStep[j].From;
                    if (front === next) {
                        condition.push(nodesStep[i].From);
                    }
                }
            }

            return condition;
        },

        /****************************************
         * 公共函数
         ****************************************/
        /*
        *查找节点Id
        */
        isContainsNode: function (array, dest) {
            for (var i = 0; i < array.length; i++) {
                if (array[i] === dest) {
                    return true;
                }
            }

            return false;
        },

        /*
         *去掉重复元素
         */
        uniqueArray: function (array) {
            for (var i = 0; i < array.length; i++) {
                for (var j = i + 1; j < array.length; j++) {
                    if (array[i] === array[j]) {
                        array.splice(j, 1);
                        j--;
                    }
                }
            }
            return array;
        },

    }

    u.workflow = workflow;
})(CC);