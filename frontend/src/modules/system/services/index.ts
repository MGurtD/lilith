import { ApiKeyService } from "./apikey.service";
import { UserService } from "./user.service";

export default {
  ApiKey: new ApiKeyService(),
  User: new UserService(),
};
