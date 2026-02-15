import { AuthenticationService } from "./authentications.service";
import { FileService } from "./file.service";
import { ReportService } from "./report.service";
import { RoleService } from "./role.service";
import { UserService } from "./user.service";
import { UserFilterService } from "./userfilter.service";

export default {
  Authentication: new AuthenticationService(),
  File: new FileService(),
  User: new UserService(),
  UserFilter: new UserFilterService(),
  Role: new RoleService(),
  Reports: new ReportService(),
};
