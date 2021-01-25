import { PaginationParams } from "./paginationParams"

export class LikeParams extends PaginationParams {
  predicate: string;
  userId: number;
}
