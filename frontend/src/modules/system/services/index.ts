import { ApiKeyService } from "./apikey.service";
import { UserService } from "./user.service";
import { DataMigrationService } from "./datamigration.service";

export default {
  ApiKey: new ApiKeyService(),
  User: new UserService(),
  DataMigration: new DataMigrationService(),
};
