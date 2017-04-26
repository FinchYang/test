var CC = CC || {};
(function (u, undefined) {
    var mail = {
        //作用：取出metadataAlias.js 中MFBuiltInObjectTypeAssignment 所有值
        //参数：【vault】vault对象
        GetArray: function (vault) {
            var res = []; 
            
            var array = md.mail.propDefs;
         
            var temp = [array[md.mail.propDefs.PropMailSender], array[md.mail.propDefs.PropMailSubject]];

            for (var item in temp) {
                var propId = MF.alias.propertyDef(vault, temp[item]);
                res.push(propId);
            }  
            return res;
        },

        //作用：获取未读邮件
        //参数：vault
        getUnReadEmails: function (vault) {
            var sConditons = MFiles.CreateInstance("SearchConditions");
            var condition = MFiles.CreateInstance("SearchCondition");
            condition.ConditionType = MFConditionTypeEqual;
            condition.Expression.DataPropertyValuePropertyDef =
                MF.alias.propertyDef(vault, md.mail.propDefs.PropIsRead);
            condition.TypedValue.SetValue(MFDatatypeBoolean, false);
            sConditons.Add(-1, condition);
            return MF.ObjectOps.SearchObjects(vault, 0, sConditons);
        },

        //作用：获取选择对象所在的文件夹（发件箱/收件箱/草稿箱）
        //参数：vault，对象版本和属性信息
        getSelFolderName: function (vault, objVersAndPros) {
            if (objVersAndPros.Count > 0) {
                var classId = MF.alias.classType(vault, md.mail.classAlias);
                var item = objVersAndPros.Item(1);
                if (item.VersionData.Class === classId) {
                    var props = item.Properties;
                    var folderPropId = MF.alias.propertyDef(vault, md.mail.propDefs.PropMailFolders);
                    return props.SearchForProperty(folderPropId).Value.DisplayValue;
                }
            }
            return "";
        }
    };
    u.mail = mail;
})(CC);