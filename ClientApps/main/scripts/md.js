/*
 * 别名配置文件
 */
var md = md || {};
(function(u, undefined){
	var valueList ={//值列表
		Specialty: "VlContractedProfession",
		Technology: "VLIsQualified",
		Subtechnology: "VlProvince",
		Project:"VlCity",
		Other:"VlProvince"
	};
	
	var documents={//文档
		Alias: "0",
		ClassList:{
			// Awards:{//获奖情况
			// 	Alias: "ClassAwards",
			// 	Properties:{
			// 		Specialty: "PropSpecialty",//专业
			// 		Technology: "PropTechnology",//技术
			// 		Subtechnology: "PropSubtechnology",//技术子值
			// 		Project: "PropProject",//项目
			// 		Other: "PropOther"//其他
			// 	}
			// },
			// Patent: {//知识产权
			// 	Alias: "ClassPatent",
			// 	Properties:{
			// 		Specialty: "PropSpecialty",//专业
			// 		Technology: "PropTechnology",//技术
			// 		Subtechnology: "PropSubtechnology",//技术子值
			// 		Project: "PropProject",//项目
			// 		Other: "PropOther"//其他
			// 	}
			// },
			// Thesis: {//论文
			// 	Alias: "ClassThesis",
			// 	Properties:{
			// 		Specialty: "PropSpecialty",//专业
			// 		Technology: "PropTechnology",//技术
			// 		Subtechnology: "PropSubtechnology",//技术子值
			// 		Project: "PropProject",//项目
			// 		Other: "PropOther"//其他
			// 	}
			// },
			// 
			// Technology: {//技术体系
			// 	Alias: "ClassTechnology",
			// 	Properties:{
			// 		Specialty: "PropSpecialty",//专业
			// 		Technology: "PropTechnology",//技术
			// 		Subtechnology: "PropSubtechnology",//技术子值
			// 		Project: "PropProject",//项目
			// 		Other: "PropOther"//其他
			// 	}
			// },
			// 
			// Training: {//培训教材
			// 	Alias: "ClassTraining",
			// 	Properties:{
			// 		Specialty: "PropSpecialty",//专业
			// 		Technology: "PropTechnology",//技术
			// 		Subtechnology: "PropSubtechnology",//技术子值
			// 		Project: "PropProject",//项目
			// 		Other: "PropOther"//其他
			// 	}
			// },
			// Criterion: {//团队标准
			// 	Alias: "ClassCriterion",
			// 	Properties:{
			// 		Specialty: "PropSpecialty",//专业
			// 		Technology: "PropTechnology",//技术
			// 		Subtechnology: "PropSubtechnology",//技术子值
			// 		Project: "PropProject",//项目
			// 		Other: "PropOther"//其他
			// 	}
			// },
			// Component: {//团队族库
			// 	Alias: "ClassComponent",
			// 	Properties:{
			// 		Specialty: "PropSpecialty",//专业
			// 		Technology: "PropTechnology",//技术
			// 		Subtechnology: "PropSubtechnology",//技术子值
			// 		Project: "PropProject",//项目
			// 		Other: "PropOther"//其他
			// 	}
			// },
			Projectcase: {//项目案例
				Alias: "ClassContractor",
				Properties:{
					Specialty: "PropContractorName",//专业
					Technology: "PropContractedProfession",//技术
					Subtechnology: "PropBusinessLicenseNumber",//技术子值
					Project: "PropTaxRegistrationNumber",//项目
					Other: "PropQualificationCertificateNumber"//其他
				}
			}
			
		}
	};
	
	 u.valueList = valueList;
	 u.documents = documents;
})(md);