export type AppUser = {
  id: string;
  name: string;
  surname: string;
  email?: string;
  birthDate: string;
  address?: string;
  role?: string;
};

export const DATA: readonly AppUser[] = [
  {
    id: '728ed52a',
    name: 'Masum',
    surname: 'Ulu',
    email: 'masumulu@allegorysoft.com',
    birthDate: '1990-05-14',
    address: 'adress-1',
  },
  {
    id: '728ed52b',
    name: 'Ahmet Faruk',
    surname: 'Ulu',
    email: 'ahmetfarukulu@allegorysoft.com',
    birthDate: '1992-08-21',
  },
  {
    id: '728ed52c',
    name: 'Seda',
    surname: 'Ulu',
    email: 'sedaulu@allegorysoft.com',
    birthDate: '1994-03-09',
  },
  {
    id: '728ed52d',
    name: 'Deniz',
    surname: 'Yilmaz',
    email: 'deniz.yilmaz@example.com',
    birthDate: '1991-11-02',
  },
  {
    id: '728ed52e',
    name: 'Elif',
    surname: 'Kaya',
    email: 'elif.kaya@example.com',
    birthDate: '1996-06-18',
  },
  {
    id: '728ed52f',
    name: 'Emre',
    surname: 'Demir',
    email: 'emre.demir@example.com',
    birthDate: '1989-01-27',
  },
  {
    id: '728ed530',
    name: 'Zeynep',
    surname: 'Celik',
    email: 'zeynep.celik@example.com',
    birthDate: '1993-09-12',
  },
  {
    id: '728ed531',
    name: 'Can',
    surname: 'Yildiz',
    email: 'can.yildiz@example.com',
    birthDate: '1995-04-05',
  },
  {
    id: '728ed532',
    name: 'Ece',
    surname: 'Aydin',
    email: 'ece.aydin@example.com',
    birthDate: '1997-12-23',
  },
  {
    id: '728ed533',
    name: 'Burak',
    surname: 'Ergün',
    email: 'burak.ergun@example.com',
    birthDate: '1990-07-30',
  },
  {
    id: '728ed534',
    name: 'Derya',
    surname: 'Dogan',
    email: 'derya.dogan@example.com',
    birthDate: '1992-02-16',
  },
  {
    id: '728ed535',
    name: 'Mert',
    surname: 'Kilic',
    email: 'mert.kilic@example.com',
    birthDate: '1994-10-08',
  },
  {
    id: '728ed536',
    name: 'Selin',
    surname: 'Aslan',
    email: 'selin.aslan@example.com',
    birthDate: '1991-05-25',
  },
  {
    id: '728ed537',
    name: 'Ozan',
    surname: 'Tekin',
    email: 'ozan.tekin@example.com',
    birthDate: '1996-08-03',
  },
  {
    id: '728ed538',
    name: 'Irem',
    surname: 'Aksoy',
    email: 'irem.aksoy@example.com',
    birthDate: '1993-01-19',
  },
];
