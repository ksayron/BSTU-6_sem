// 1
// MongoDB Server + MongoDB Compass installation is done outside script.

// 2
db = db.getSiblingDB('knp_hr_mongo');

// 3
db.people.drop();
db.business_trips.drop();

db.people.insertMany([
  {
    person_id: 1,
    first_name: "Ivan",
    last_name: "Petrov",
    middle_name: "Sergeevich",
    department: "IT",
    position: "Developer",
    skills: ["Java", "SQL", "MongoDB"],
    contacts: { email: "ivan.petrov@corp.by", phone: "+375291111111" },
    salary: 3200,
    birth_date: ISODate("1999-04-12T00:00:00Z"),
    is_active: true,
    notes: "backend"
  },
  {
    person_id: 2,
    first_name: "Anna",
    last_name: "Kovaleva",
    middle_name: "Igorevna",
    department: "HR",
    position: "HR Specialist",
    skills: ["Interview", "Onboarding"],
    contacts: { email: "anna.kovaleva@corp.by" },
    salary: 2400,
    birth_date: ISODate("2000-07-03T00:00:00Z"),
    is_active: true,
    notes: "recruiter"
  },
  {
    person_id: 3,
    first_name: "Pavel",
    last_name: "Sidorov",
    middle_name: "Olegovich",
    department: "Finance",
    position: "Accountant",
    skills: ["Excel", "1C"],
    contacts: { email: "p.sidorov@corp.by", phone: "+375292222222" },
    salary: 2800,
    birth_date: ISODate("1997-11-21T00:00:00Z"),
    is_active: false,
    notes: null
  },
  {
    person_id: 4,
    first_name: "Maria",
    last_name: "Lebedeva",
    middle_name: "Andreevna",
    department: "IT",
    position: "QA Engineer",
    skills: ["Testing", "SQL", "Postman"],
    contacts: { email: "maria.lebedeva@corp.by", phone: "+375293333333" },
    salary: 2900,
    birth_date: ISODate("2001-02-18T00:00:00Z"),
    is_active: true,
    notes: "manual+api"
  },
  {
    person_id: 5,
    first_name: "Nikita",
    last_name: "Orlov",
    middle_name: "Denisovich",
    department: "Sales",
    position: "Sales Manager",
    skills: ["Negotiation", "CRM"],
    contacts: { email: "nikita.orlov@corp.by" },
    salary: 2600,
    birth_date: ISODate("1998-09-09T00:00:00Z"),
    is_active: true
  }
]);

db.business_trips.insertMany([
  {
    trip_id: 101,
    person_id: 1,
    destination: "Warsaw",
    country: "Poland",
    start_date: ISODate("2026-03-01T00:00:00Z"),
    end_date: ISODate("2026-03-06T00:00:00Z"),
    purpose: "Client integration",
    cost: 1200,
    tags: ["client", "integration"],
    approved: true
  },
  {
    trip_id: 102,
    person_id: 4,
    destination: "Berlin",
    country: "Germany",
    start_date: ISODate("2026-03-10T00:00:00Z"),
    end_date: ISODate("2026-03-13T00:00:00Z"),
    purpose: "QA conference",
    cost: 900,
    tags: ["conference", "qa"],
    approved: true
  },
  {
    trip_id: 103,
    person_id: 2,
    destination: "Vilnius",
    country: "Lithuania",
    start_date: ISODate("2026-03-15T00:00:00Z"),
    end_date: ISODate("2026-03-16T00:00:00Z"),
    purpose: "HR meetup",
    cost: 400,
    tags: ["hr"],
    approved: false
  },
  {
    trip_id: 104,
    person_id: 1,
    destination: "Amsterdam",
    country: "Netherlands",
    start_date: ISODate("2026-04-02T00:00:00Z"),
    end_date: ISODate("2026-04-08T00:00:00Z"),
    purpose: "Platform workshop",
    cost: 1600,
    tags: ["workshop", "platform"],
    approved: true
  },
  {
    trip_id: 105,
    person_id: 5,
    destination: "Prague",
    country: "Czech Republic",
    start_date: ISODate("2026-04-12T00:00:00Z"),
    end_date: ISODate("2026-04-14T00:00:00Z"),
    purpose: "Sales demo",
    cost: 700,
    tags: ["sales", "demo"],
    approved: true
  }
]);

db.people.updateOne(
  { person_id: 2 },
  { $set: { salary: 2550, "contacts.phone": "+375294444444" } }
);

db.people.updateMany(
  { department: "IT" },
  { $addToSet: { skills: "Git" } }
);

db.business_trips.updateOne(
  { trip_id: 103 },
  { $set: { approved: true, purpose: "HR partner meeting" } }
);

// 4
print("4.1 condition ($gt, $and)");
db.people.find({ $and: [ { salary: { $gt: 2700 } }, { is_active: true } ] }).pretty();

print("4.2 array operator ($all)");
db.people.find({ skills: { $all: ["SQL", "Git"] } }).pretty();

print("4.3 $exists");
db.people.find({ "contacts.phone": { $exists: true } }).pretty();

print("4.4 $type");
db.people.find({ salary: { $type: "double" } }).pretty();

print("4.5 $regex");
db.people.find({ last_name: { $regex: "ov$", $options: "i" } }).pretty();

// 5
print("5.1 projection include");
db.people.find(
  { department: "IT" },
  { _id: 0, person_id: 1, first_name: 1, last_name: 1, skills: 1 }
).pretty();

print("5.2 projection nested");
db.business_trips.find(
  { approved: true },
  { _id: 0, trip_id: 1, destination: 1, country: 1, cost: 1, tags: 1 }
).pretty();

// 6
print("6.1 count all");
print(db.people.count());

print("6.2 count by filter");
print(db.business_trips.count({ approved: true }));

// 7
print("7.1 limit");
db.people.find({}, { _id: 0, person_id: 1, last_name: 1 }).sort({ person_id: 1 }).limit(3).pretty();

print("7.2 skip + limit");
db.people.find({}, { _id: 0, person_id: 1, last_name: 1 }).sort({ person_id: 1 }).skip(2).limit(2).pretty();

// 8
print("8.1 distinct departments");
printjson(db.people.distinct("department"));

print("8.2 distinct countries with approved=true");
printjson(db.business_trips.distinct("country", { approved: true }));

// 9
print("9.1 aggregate totals by department");
db.people.aggregate([
  { $group: {
      _id: "$department",
      employees_count: { $sum: 1 },
      avg_salary: { $avg: "$salary" },
      max_salary: { $max: "$salary" }
  }},
  { $sort: { employees_count: -1, _id: 1 } }
]);

print("9.2 aggregate trip costs by person");
db.business_trips.aggregate([
  { $group: {
      _id: "$person_id",
      trips_count: { $sum: 1 },
      total_cost: { $sum: "$cost" },
      avg_cost: { $avg: "$cost" }
  }},
  { $sort: { total_cost: -1 } }
]);

// 10
print("10.1 $match empty filter + group by multiple keys");
db.business_trips.aggregate([
  { $match: {} },
  { $group: {
      _id: { country: "$country", approved: "$approved" },
      trips_count: { $sum: 1 },
      total_cost: { $sum: "$cost" }
  }},
  { $sort: { "_id.country": 1, "_id.approved": 1 } }
]);

print("10.2 $match non-empty filter + group by multiple keys");
db.business_trips.aggregate([
  { $match: { approved: true, cost: { $gte: 700 } } },
  { $group: {
      _id: { country: "$country", person_id: "$person_id" },
      trips_count: { $sum: 1 },
      total_cost: { $sum: "$cost" }
  }},
  { $sort: { total_cost: -1 } }
]);

