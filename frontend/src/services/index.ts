import { ApiKeyService } from "@/modules/system/services/apikey.service";
import { AuthenticationService } from "./authentications.service";
import { FileService } from "./file.service";
import { ReportService } from "./report.service";
import { RoleService } from "./role.service";
import { UserService } from "@/modules/system/services/user.service";
import { UserFilterService } from "./userfilter.service";
import { UserTableViewService } from "./usertableview.service";

export default {
  ApiKey: new ApiKeyService(),
  Authentication: new AuthenticationService(),
  File: new FileService(),
  User: new UserService(),
  UserFilter: new UserFilterService(),
  UserTableView: new UserTableViewService(),
  Role: new RoleService(),
  Reports: new ReportService(),
};
