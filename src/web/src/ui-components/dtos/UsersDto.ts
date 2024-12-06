export interface UsersDto {
    items: UserDto[];
}
export interface UserDto {
    id: string;
    name: string;
    firstName?: string;
    lastName?: string;
    jobTitle?: string;
    roles: string[]
}