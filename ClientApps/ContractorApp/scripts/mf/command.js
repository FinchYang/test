/************************************************
* M-Files CustomCommand Wrapper
* export name: MF.Command
*************************************************/
var MF = MF || {};
(function (u) {

    function MfCommand(name, iconPath) {
        ///<summary>对应M-Files中的自定义命令</summary>
        ///<param name="name" type="string">命令的名称</param>
        ///<param name="iconPath" type="string">外部的ICON路径</param>
        this.name = name;
        this.command = null; // CustomCommand Id
        this.iconPath = iconPath || '';
        this.callbacks = []; //event sinks
        this.taskgroupIds = []; //command in taskgroups
        this.contextmenuIds = []; //command in context menu
    }

    MfCommand.prototype.create = function(shellFrame) {
        ///<summary>创建自定义命令，若已存在，则不做任何操作</summary>
        if (!this.name) {
            throw new Error('must provide command name!');
        }
        if (!this.command && this.command !== 0) {
            this.command = shellFrame.Commands.CreateCustomCommand(this.name);
            if (this.iconPath) {
                shellFrame.Commands.SetIconFromPath(this.command, this.iconPath);
            }
        }
    };

    MfCommand.prototype.registerEvent = function(shellFrame, callback) {
        ///<summary>注册事件</summary>
        ///<param name="callback" type="function">以shellFrame作为参数的函数</param>
        if (!this.command && this.command !== 0) {
            return;
        }
        if (!callback || typeof callback !== 'function') {
            return;
        }
        var that = this;
        var sink = shellFrame.Commands.Events.Register(Event_CustomCommand, function(cmdId) {
            if (cmdId === that.command) {
                callback.call(that, shellFrame);
            }
        });
        this.callbacks.push(sink);
    };

    MfCommand.prototype.unregisterEvent = function(shellFrame, eventSink) {
        ///<param name="eventSink" type="long">Commands.Events.Register函数的返回值</param>
        if (eventSink || eventSink === 0) {
            for (var i = 0; i < this.callbacks.length; i++) {
                if (eventSink === this.callbacks[i]) {
                    shellFrame.Commands.Unregister(eventSink);
                    return;
                }
            }
        }
    };

    MfCommand.prototype.remove = function(shellFrame) {
        ///<summary>删除自定义命令，所有加到菜单和任务组的命令都将消失</summary>
        if (!this.command && this.command !== 0) {
            return;
        }
        shellFrame.Commands.DeleteCustomCommand(this.command);
    };

    MfCommand.prototype.add2TaskGroup = function(shellFrame, groupId, orderPriority) {
        ///<param name="groupId" type="TaskPaneGroup or Long"></param>
        if (!this.command && this.command !== 0) {
            return;
        }
        if (!groupId && groupId !== 0) {
            throw new Error('非法的任务栏组ID：' + groupId);
        }
        for (var i = 0; i < this.taskgroupIds.length; i++) {
            if (this.taskgroupIds[i] === groupId) {
                return;
            }
        }
        if (!orderPriority && orderPriority !== 0) {
            orderPriority = 0;
        }
        shellFrame.TaskPane.AddCustomCommandToGroup(this.command, groupId, orderPriority);
        this.taskgroupIds.push(groupId);
    };

    MfCommand.prototype.removeFromTaskGroup = function(shellFrame, groupId) {
        ///<param name="groupId" type="TaskPaneGroup or Long"></param>
        if (!this.command && this.command !== 0) {
            return;
        }
        if (!groupId && groupId !== 0) {
            return;
        }
        for (var i = 0; i < this.taskgroupIds.length; i++) {
            if (this.taskgroupIds[i] === groupId) {
                shellFrame.TaskPane.RemoveCustomCommandFromGroup(this.command, groupId);
                this.taskgroupIds.splice(i, 1);
                return;
            }
        }
    };

    MfCommand.prototype.add2ContextMenu = function(shellFrame, locationId, orderPriority) {
        ///<param name="locationId" type="MenuLocation"></param>
        if (!this.command && this.command !== 0) {
            return;
        }
        if (!locationId && locationId !== 0) {
            throw new Error('非法的菜单位置ID：' + locationId);
        }
        for (var i = 0; i < this.contextmenuIds.length; i++) {
            if (this.contextmenuIds[i] === locationId) {
                return;
            }
        }
        if (!orderPriority && orderPriority !== 0) {
            orderPriority = 0;
        }
        shellFrame.Commands.AddCustomCommandToMenu(this.command, locationId, orderPriority);
        this.contextmenuIds.push(locationId);
    };

    MfCommand.prototype.removeFromContextMenu = function(shellFrame, locationId) {
        ///<param name="locationId" type="MenuLocation"></param>
        if (!this.command && this.command !== 0) {
            return;
        }
        if (!locationId && locationId !== 0) {
            return;
        }
        for (var i = 0; i < this.contextmenuIds.length; i++) {
            if (this.contextmenuIds[i] === locationId) {
                shellFrame.Commands.RemoveCustomCommandFromMenu(this.command, locationId);
                this.contextmenuIds.splice(i, 1);
                return;
            }
        }
    };

    u.Command = MfCommand;
}(MF));